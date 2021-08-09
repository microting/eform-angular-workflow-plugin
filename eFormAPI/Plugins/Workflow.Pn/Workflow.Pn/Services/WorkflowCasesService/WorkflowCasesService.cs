/*
The MIT License (MIT)
Copyright (c) 2007 - 2021 Microting A/S
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.IO;
using System.Reflection;
using eFormCore;
using ImageMagick;
using Microting.eForm.Dto;
using Microting.eForm.Helpers;
using Microting.eForm.Infrastructure.Models;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using Microting.eFormWorkflowBase.Messages;
using Workflow.Pn.Helpers;

namespace Workflow.Pn.Services.WorkflowCasesService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.Models;
    using Infrastructure.Models.Cases;
    using Infrastructure.Models.Settings;
    using Microsoft.EntityFrameworkCore;
    using Microting.eForm.Infrastructure.Constants;
    using Microting.eForm.Infrastructure.Data.Entities;
    using Microting.eFormApi.BasePn.Abstractions;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Common;
    using Microting.eFormWorkflowBase.Infrastructure.Const;
    using Microting.eFormWorkflowBase.Infrastructure.Data;
    using Rebus.Bus;
    using RebusService;
    using WorkflowLocalizationService;
    using CheckListValue = Microting.eForm.Infrastructure.Models.CheckListValue;

    public class WorkflowCasesService : IWorkflowCasesService
    {
        private readonly IEFormCoreService _coreHelper;
        private readonly IWorkflowLocalizationService _workflowLocalizationService;
        private readonly IPluginDbOptions<WorkflowBaseSettings> _options;
        private readonly IUserService _userService;
        private readonly IBus _bus;
        private readonly WorkflowPnDbContext _workflowPnDbContext;

        public WorkflowCasesService(IEFormCoreService coreHelper,
            IUserService userService,
            IRebusService rebusService,
            WorkflowPnDbContext workflowPnDbContext,
            IWorkflowLocalizationService workflowLocalizationService, IPluginDbOptions<WorkflowBaseSettings> options)
        {
            _coreHelper = coreHelper;
            _userService = userService;
            _workflowLocalizationService = workflowLocalizationService;
            _options = options;
            _workflowPnDbContext = workflowPnDbContext;
            _bus = rebusService.GetBus();
        }

        public async Task<OperationDataResult<Paged<WorkflowCasesModel>>> Index(WorkflowCasesResponse request)
        {
            // get query
            var query = _workflowPnDbContext.WorkflowCases
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed);

            // add filtering
            if (!string.IsNullOrEmpty(request.NameFilter))
            {
                query = QueryHelper
                    .AddFilterToQuery(query, new List<string>
                        {
                            "Id",
                            "IncidentType",
                            "ActionPlan",
                            "Description",
                            "IncidentPlace",
                            "SolvedBy",
                            "IncidentType",
                            "Status"
                        }, request.NameFilter);
            }

            // add sorting
            query = QueryHelper.AddSortToQuery(query, request.Sort, request.IsSortDsc);

            // get total
            var total = await query.Select(x => x.Id).CountAsync();

            var workflowCases = new List<WorkflowCasesModel>();
            if (total > 0)
            {
                // pagination
                query = query.Skip(request.Offset)
                    .Take(request.PageSize);

                var siteNames = await query
                    .Select(x => x.SolvedBy)
                    .Distinct()
                    .ToListAsync();

                var core = await _coreHelper.GetCore();
                var sdkDbContext = core.DbContextHelper.GetDbContext();
                var idsSites = sdkDbContext.Sites
                    .Where(x => siteNames.Contains(x.Name))
                    .Select(x => new { x.Id, x.Name })
                    .ToList();

                // add select to query and get from db
                workflowCases = await query
                    .Select(x => new WorkflowCasesModel
                    {
                        ActionPlan = x.ActionPlan,
                        DateOfIncident = x.DateOfIncident,
                        Deadline = x.Deadline,
                        Description = x.Description,
                        Id = x.Id,
                        IncidentPlace = x.IncidentPlace,
                        IncidentType = x.IncidentType,
                        PhotosExist = x.PhotosExist,
                        StatusName = x.Status,
                        //it comment because it posible System.InvalidOperationException: The LINQ expression 'y' could not be translated.
                        //Status = WorkflowCaseStatuses.Statuses.FirstOrDefault(y => y.Key == x.Status).Value,
                        //ToBeSolvedById = idsSites.FirstOrDefault(y => y.Name == x.SolvedBy).Id,
                        ToBeSolvedBy = x.SolvedBy,
                        UpdatedAt = (DateTime)x.UpdatedAt,
                    })
                    .ToListAsync();

                foreach (var workflowCasesModel in workflowCases)
                {
                    if (workflowCasesModel.StatusName != null)
                    {
                        workflowCasesModel.Status =
                            WorkflowCaseStatuses.Statuses.First(y => y.Key == workflowCasesModel.StatusName).Value;
                        if (!string.IsNullOrEmpty(workflowCasesModel.ToBeSolvedBy))
                        {
                            workflowCasesModel.ToBeSolvedById =
                                idsSites.First(y => y.Name == workflowCasesModel.ToBeSolvedBy).Id;
                        }
                    }
                }
            }
            return new OperationDataResult<Paged<WorkflowCasesModel>>(true, new Paged<WorkflowCasesModel> { Entities = workflowCases, Total = total });
        }

        public async Task<OperationDataResult<WorkflowCasesUpdateModel>> Read(int id)
        {
            var core = await _coreHelper.GetCore();
            var sdkDbContext = core.DbContextHelper.GetDbContext();

            // get query
            var workflowDbCase = await _workflowPnDbContext.WorkflowCases
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            if (workflowDbCase == null)
            {
                return new OperationDataResult<WorkflowCasesUpdateModel>(false, _workflowLocalizationService.GetString("WorkflowCaseNotFound"));
            }

            var incidentPlaceListId =
                await _workflowPnDbContext.PluginConfigurationValues.SingleOrDefaultAsync(x => x.Name == $"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentPlaceListId)}");

            var incidentTypeListId = await _workflowPnDbContext.PluginConfigurationValues.SingleOrDefaultAsync(x =>
                x.Name == $"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentTypeListId)}");

            WorkflowCasesUpdateModel workflowCase = new WorkflowCasesUpdateModel
            {
                ActionPlan = workflowDbCase.ActionPlan,
                DateOfIncident = workflowDbCase.DateOfIncident.ToString("yyyy-MM-dd"), // 2021-08-29,
                Deadline = workflowDbCase.Deadline?.ToString("yyyy-MM-dd"),
                Description = workflowDbCase.Description,
                IncidentPlace = workflowDbCase.IncidentPlace,
                IncidentPlaceId = workflowDbCase.IncidentPlaceId,
                IncidentPlaceListId = incidentPlaceListId.Value,
                IncidentType = workflowDbCase.IncidentType,
                IncidentTypeId = workflowDbCase.IncidentTypeId,
                IncidentTypeListId = incidentTypeListId.Value,
                Id = workflowDbCase.Id
            };

            var bla = await _workflowPnDbContext.PicturesOfTasks
                .Where(x => x.WorkflowCaseId == workflowDbCase.Id
                            && x.WorkflowState != Constants.WorkflowStates.Removed).ToListAsync();

            int i = bla.Count;
            foreach (PicturesOfTask picturesOfTask in bla)
            {
                var result = await sdkDbContext.UploadedDatas.SingleOrDefaultAsync(x =>
                    x.Id == picturesOfTask.UploadedDataId && x.WorkflowState != Constants.WorkflowStates.Removed);

                if (result != null)
                {
                    Infrastructure.Models.FieldValue fieldValue = new Infrastructure.Models.FieldValue()
                    {
                        Id = picturesOfTask.Id,
                        Longitude = picturesOfTask.Longitude,
                        Latitude = picturesOfTask.Latitude,
                        UploadedDataObj = new UploadedDataObj()
                        {
                            Id = result.Id,
                            FileName = picturesOfTask.FileName
                        }
                    };
                    workflowCase.PicturesOfTask.Add(fieldValue);
                }
                else
                {
                    await picturesOfTask.Delete(_workflowPnDbContext);
                }
            }

            if (i == 0)
            {
                workflowDbCase.PhotosExist = false;
                await workflowDbCase.Update(_workflowPnDbContext);
            }

            if (!string.IsNullOrEmpty(workflowDbCase.Status))
            {
                workflowCase.Status = WorkflowCaseStatuses.Statuses.First(y => y.Key == workflowDbCase.Status).Value;
            }

            if (!string.IsNullOrEmpty(workflowDbCase.SolvedBy))
            {
                workflowCase.ToBeSolvedById = sdkDbContext.Sites.First(y => y.Name == workflowDbCase.SolvedBy).Id;
            }

            return new OperationDataResult<WorkflowCasesUpdateModel>(true, workflowCase);
        }

        public async Task<OperationResult> UpdateWorkflowCase(WorkflowCasesUpdateModel model)
        {
            var core = await _coreHelper.GetCore();
            try
            {
                var solvedByNotSelected = !model.ToBeSolvedById.HasValue;
                var statusClosed = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "Closed").Value;
                var statusOngoing = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "Ongoing").Value;

                var workflowCase = await _workflowPnDbContext.WorkflowCases.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (workflowCase == null)
                {
                    return new OperationResult(false, _workflowLocalizationService.GetString("WorkflowCaseNotFound"));
                }

                var sdkDbContext = core.DbContextHelper.GetDbContext();
                var solvedUser = await sdkDbContext.Sites.Where(x => x.Id == model.ToBeSolvedById)
                    .Select(x => new
                    {
                        x.Name,
                        x.LanguageId
                    })
                    .FirstOrDefaultAsync();
                if (!solvedByNotSelected)
                {
                    workflowCase.SolvedBy = solvedUser.Name;
                }

                if (model.Status.HasValue)
                {
                    workflowCase.Status = WorkflowCaseStatuses.Statuses.First(x => x.Value == model.Status).Key;
                }
                workflowCase.UpdatedByUserId = _userService.UserId;
                workflowCase.Deadline = string.IsNullOrEmpty(model.Deadline)  ? null : DateTime.Parse(model.Deadline);
                workflowCase.IncidentPlace = model.IncidentPlace;
                if (model.IncidentPlaceId != null) workflowCase.IncidentPlaceId = (int) model.IncidentPlaceId;
                workflowCase.IncidentType = model.IncidentType;
                if (model.IncidentTypeId != null) workflowCase.IncidentTypeId = (int) model.IncidentTypeId;
                //if(model.IncidentPlace.HasValue)
                // {
                //     workflowCase.IncidentPlace = await sdkDbContext.FieldOptionTranslations
                //         .Where(x => x.Id == model.IncidentPlace)
                //         .Select(x => x.Text)
                //         .FirstAsync();
                // }
                workflowCase.ActionPlan = model.ActionPlan;
                workflowCase.Description = model.Description;
                workflowCase.DateOfIncident = DateTime.Parse(model.DateOfIncident);

                await workflowCase.Update(_workflowPnDbContext);

                switch (solvedByNotSelected)
                {
                    case true when statusClosed:
                        // send email with pdf report to device user
                        await _bus.SendLocal(new QueueEformEmail
                        {
                            CaseId = workflowCase.Id,
                            // UserName = await _userService.GetCurrentUserFullName(),
                            // CurrentUserLanguageId = _userService.GetCurrentUserLanguage().GetAwaiter().GetResult().Id,
                            // SolvedUser = new List<KeyValuePair<string, int>>()
                        });

                        if (workflowCase.DeployedMicrotingUid != null)
                        {
                            await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                        }

                        workflowCase.DeployedMicrotingUid = null;
                        await workflowCase.Update(_workflowPnDbContext);
                        break;
                    case false when statusClosed:
                        {
                            // var language = await sdkDbContext.Languages.FirstAsync(x => x.Id == solvedUser.LanguageId);
                            // var solvedUsers = new List<KeyValuePair<string, int>>
                            // {
                            //     new(solvedUser.Name, language.Id),
                            // };

                            // send email with pdf report to device user and solved user
                            await _bus.SendLocal(new QueueEformEmail
                            {
                                CaseId = workflowCase.Id,
                                // UserName = await _userService.GetCurrentUserFullName(),
                                // CurrentUserLanguageId = _userService.GetCurrentUserLanguage().GetAwaiter().GetResult().Id,
                                // SolvedUser = solvedUsers
                            });

                            if (workflowCase.DeployedMicrotingUid != null)
                            {
                                await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                            }

                            workflowCase.DeployedMicrotingUid = null;
                            await workflowCase.Update(_workflowPnDbContext);
                            break;
                        }
                    case false when statusOngoing:
                        {

                            // Docx and PDF files
                            string timeStamp = DateTime.UtcNow.ToString("yyyyMMdd") + "_" + DateTime.UtcNow.ToString("hhmmss");
                            string downloadPath = Path.Combine(Path.GetTempPath(), "pdf");
                            string docxFileName = $"{timeStamp}{model.ToBeSolvedById}_temp.docx";
                            string tempPDFFileName = $"{timeStamp}{model.ToBeSolvedById}_temp.pdf";
                            string tempPDFFilePath = Path.Combine(downloadPath, tempPDFFileName);

                            var resourceString = "Workflow.Pn.Resources.Templates.page.html";
                            var assembly = Assembly.GetExecutingAssembly();
                            string html;
                            await using (var resourceStream = assembly.GetManifestResourceStream(resourceString))
                            {
                                using var reader = new StreamReader(resourceStream ?? throw new InvalidOperationException($"{nameof(resourceStream)} is null"));
                                html = await reader.ReadToEndAsync();
                            }

                            // Read docx stream
                            resourceString = "Workflow.Pn.Resources.Templates.file.docx";
                            var docxFileResourceStream = assembly.GetManifestResourceStream(resourceString);
                            if (docxFileResourceStream == null)
                            {
                                throw new InvalidOperationException($"{nameof(docxFileResourceStream)} is null");
                            }

                            var docxFileStream = new MemoryStream();
                            await docxFileResourceStream.CopyToAsync(docxFileStream);
                            await docxFileResourceStream.DisposeAsync();
                            string basePicturePath = Path.Combine(Path.GetTempPath(), "pictures");
                            var word = new WordProcessor(docxFileStream);
                            string imagesHtml = "";
                            var picturesOfTasks = new List<string>();

                            var pictures =
                                await _workflowPnDbContext.PicturesOfTasks.Where(x =>
                                    x.WorkflowCaseId == workflowCase.Id).ToListAsync();

                            foreach (PicturesOfTask picturesOfTask in pictures)
                            {
                                picturesOfTasks.Add(picturesOfTask.FileName);
                            }

                            foreach (var imagesName in picturesOfTasks)
                            {
                                Console.WriteLine($"Trying to insert image into document : {imagesName}");
                                imagesHtml = await InsertImage(core, imagesName, imagesHtml, 700, 650, basePicturePath);
                            }

                            html = html.Replace("{%Content%}", imagesHtml);

                            word.AddHtml(html);
                            word.Dispose();
                            docxFileStream.Position = 0;

                            // Build docx
                            await using (var docxFile = new FileStream(docxFileName, FileMode.Create, FileAccess.Write))
                            {
                                docxFileStream.WriteTo(docxFile);
                            }

                            // Convert to PDF
                            ReportHelper.ConvertToPdf(docxFileName, downloadPath);
                            File.Delete(docxFileName);

                            // Upload PDF
                            // string pdfFileName = null;
                            string hash = await core.PdfUpload(tempPDFFilePath);
                            if (hash != null)
                            {
                                //rename local file
                                FileInfo fileInfo = new FileInfo(tempPDFFilePath);
                                fileInfo.CopyTo(downloadPath + hash + ".pdf", true);
                                fileInfo.Delete();
                                await core.PutFileToStorageSystem(Path.Combine(downloadPath, $"{hash}.pdf"), $"{hash}.pdf");

                                // TODO Remove from file storage?
                            }

                            // eform is deployed to solver device user
                            Folder folder =
                                await sdkDbContext.Folders.SingleOrDefaultAsync(x =>
                                    x.Id == _options.Value.FolderTasksId);
                            var mainElement = await core.ReadeForm(_options.Value.SecondEformId, await _userService.GetCurrentUserLanguage());
                            mainElement.Repeated = 1;
                            mainElement.EndDate = DateTime.Now.AddYears(10).ToUniversalTime();
                            mainElement.StartDate = DateTime.Now.ToUniversalTime();
                            mainElement.CheckListFolderName = folder.MicrotingUid.ToString();
                            mainElement.PushMessageTitle = workflowCase.IncidentType;

                            var dataElement = mainElement.ElementList[0] as DataElement;
                            mainElement.Label = workflowCase.IncidentType;
                            dataElement.Label = workflowCase.IncidentType;
                            dataElement.Description = new CDataValue()
                            {
                                InderValue = $"{workflowCase.IncidentPlace}<br><strong>Deadline:</strong> {workflowCase.Deadline?.ToString("dd.MM.yyyy")}"
                            };
                            var info = dataElement!.DataItemList[0];
                            mainElement.PushMessageBody = $"{workflowCase.IncidentPlace}\nDeadline: {workflowCase.Deadline?.ToString("dd.MM.yyyy")}";

                            info.Label = $@"<strong>Event created by:</strong> {workflowCase.SolvedBy}<br>";
                            info.Label += $@"<strong>Event date:</strong> {workflowCase.DateOfIncident:dd.MM.yyyy}<br>";
                            info.Label += $@"<strong>Event:</strong> {workflowCase.IncidentType}<br>";
                            info.Label += $@"<strong>Event location:</strong> {workflowCase.IncidentPlace}<br>";
                            info.Label += $@"<strong>Deadline:</strong> {workflowCase.Deadline?.ToString("dd.MM.yyyy")}<br>";
                            info.Label += $@"<strong>Status:</strong> {workflowCase.Status}<br>";

                            dataElement!.DataItemList[0] = info;
                            mainElement.ElementList[0] = dataElement;

                            if (hash != null)
                            {
                                ((ShowPdf)dataElement.DataItemList[1]).Value = hash;
                            }

                            ((Comment) dataElement.DataItemList[2]).Value = workflowCase.Description;

                            ((Comment) dataElement.DataItemList[3]).Value = workflowCase.ActionPlan;

                            if (workflowCase.DeployedMicrotingUid != null)
                            {
                                await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                            }

                            Site site = await sdkDbContext.Sites.SingleOrDefaultAsync(x =>
                                x.Id == model.ToBeSolvedById);

                            workflowCase.DeployedMicrotingUid = (int)await core.CaseCreate(mainElement, "", (int)site.MicrotingUid, folder.Id);
                            await workflowCase.Update(_workflowPnDbContext);
                            break;
                        }
                }

                return new OperationResult(true, _workflowLocalizationService.GetString("CaseHasBeenUpdated"));
            }
            catch (Exception ex)
            {
                Log.LogException(ex.Message);
                Log.LogException(ex.StackTrace);
                return new OperationResult(false, $"{_workflowLocalizationService.GetString("CaseCouldNotBeUpdated")} Exception: {ex.Message}");
            }
        }

        public async Task<OperationDataResult<List<WorkflowPlacesModel>>> GetPlaces()
        {
            try
            {
                if (_options.Value.FirstEformId != 0)
                {
                    var core = await _coreHelper.GetCore();
                    var sdkDbContext = core.DbContextHelper.GetDbContext();

                    var fieldWithPlaces = await sdkDbContext.Fields
                        .Where(x => x.CheckListId == _options.Value.FirstEformId)
                        .Where(x => x.FieldTypeId == 8) // fieldType.id == 8; fieldType.type -> SingleSelect
                        .Select(x => x.Id)
                        .Skip(1)
                        .FirstOrDefaultAsync();

                    var languageId = await _userService.GetCurrentUserLanguage();

                    var fieldOptionTranslations = await sdkDbContext.FieldOptions
                        .Where(x => x.FieldId == fieldWithPlaces)
                        .Include(x => x.FieldOptionTranslations)
                        .SelectMany(x => x.FieldOptionTranslations)
                        .Where(x => x.LanguageId == languageId.Id)
                        .Select(x => new WorkflowPlacesModel { Name = x.Text, Id = x.Id })
                        .ToListAsync();

                    return new OperationDataResult<List<WorkflowPlacesModel>>(true, fieldOptionTranslations);
                }

                return new OperationDataResult<List<WorkflowPlacesModel>>(true, new List<WorkflowPlacesModel>());
            }
            catch (Exception ex)
            {
                Log.LogException(ex.Message);
                Log.LogException(ex.StackTrace);
                return new OperationDataResult<List<WorkflowPlacesModel>>(false, $"{_workflowLocalizationService.GetString("ErrorWhileGetPlaces")} Exception: {ex.Message}");
            }
        }

        private async Task<string> InsertImage(Core core, string imageName, string itemsHtml, int imageSize, int imageWidth, string basePicturePath)
        {

            bool s3Enabled = core.GetSdkSetting(Settings.s3Enabled).Result.ToLower() == "true";
            bool swiftEnabled = core.GetSdkSetting(Settings.swiftEnabled).Result.ToLower() == "true";
            var filePath = Path.Combine(basePicturePath, imageName);
            Stream stream;
            if (swiftEnabled)
            {
                var storageResult = await core.GetFileFromSwiftStorage(imageName);
                stream = storageResult.ObjectStreamContent;
            }
            else if (s3Enabled)
            {
                var storageResult = await core.GetFileFromS3Storage(imageName);
                stream = storageResult.ResponseStream;
            }
            else if (!File.Exists(filePath))
            {
                return null;
                // return new OperationDataResult<Stream>(
                //     false,
                //     _localizationService.GetString($"{imagesName} not found"));
            }
            else
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }

            using (var image = new MagickImage(stream))
            {
                var profile = image.GetExifProfile();
                // Write all values to the console
                foreach (var value in profile.Values)
                {
                    Console.WriteLine("{0}({1}): {2}", value.Tag, value.DataType, value.ToString());
                }
                //image.AutoOrient();
                // decimal currentRation = image.Height / (decimal)image.Width;
                // int newWidth = imageSize;
                // int newHeight = (int)Math.Round((currentRation * newWidth));
                //
                // image.Resize(newWidth, newHeight);
                // image.Crop(newWidth, newHeight);
                // if (newWidth > newHeight)
                // {
                     image.Rotate(90);
                // }
                var base64String = image.ToBase64();
                itemsHtml +=
                    $@"<p><img src=""data:image/png;base64,{base64String}"" width=""{imageWidth}px"" alt="""" /></p>";
            }

            await stream.DisposeAsync();

            return itemsHtml;
        }
    }
}

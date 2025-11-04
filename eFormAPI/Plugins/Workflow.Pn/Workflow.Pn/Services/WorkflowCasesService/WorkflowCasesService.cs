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
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using eFormCore;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Microting.eForm.Dto;
using Microting.eForm.Helpers;
using Microting.eForm.Infrastructure.Models;
using Microting.EformAngularFrontendBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Helpers;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using Microting.eFormWorkflowBase.Messages;
using QuestPDF.Fluent;
using SendGrid;
using SendGrid.Helpers.Mail;
using Sentry;
using Workflow.Pn.Helpers;
using Workflow.Pn.Infrastructure.Helpers;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using CellValue = DocumentFormat.OpenXml.Spreadsheet.CellValue;
using CellValues = DocumentFormat.OpenXml.Spreadsheet.CellValues;
using Row = DocumentFormat.OpenXml.Spreadsheet.Row;
using Sheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;
using SheetData = DocumentFormat.OpenXml.Spreadsheet.SheetData;
using Sheets = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using Workbook = DocumentFormat.OpenXml.Spreadsheet.Workbook;
using Worksheet = DocumentFormat.OpenXml.Spreadsheet.Worksheet;

namespace Workflow.Pn.Services.WorkflowCasesService;

using System;
using System.Collections.Generic;
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
using WorkflowLocalizationService;

public class WorkflowCasesService(
    IEFormCoreService coreHelper,
    IUserService userService,
    WorkflowPnDbContext workflowPnDbContext,
    IWorkflowLocalizationService workflowLocalizationService,
    IPluginDbOptions<WorkflowBaseSettings> options,
    ILogger<WorkflowCasesService> logger,
    BaseDbContext _baseDbContext)
    : IWorkflowCasesService
{

    public async Task<OperationDataResult<Paged<WorkflowCasesModel>>> Index(WorkflowCasesResponse request)
    {
        // get query
        var query = workflowPnDbContext.WorkflowCases
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

            var core = await coreHelper.GetCore();
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
                    NumberOfPhotos = x.NumberOfPhotos ?? 0,
                    //it comment because it posible System.InvalidOperationException: The LINQ expression 'y' could not be translated.
                    //Status = WorkflowCaseStatuses.Statuses.FirstOrDefault(y => y.Key == x.Status).Value,
                    //ToBeSolvedById = idsSites.FirstOrDefault(y => y.Name == x.SolvedBy).Id,
                    SolvedBy = x.SolvedBy,
                    UpdatedAt = (DateTime)x.UpdatedAt,
                    CreatedBySiteName = x.CreatedBySiteName
                })
                .ToListAsync();

            foreach (var workflowCasesModel in workflowCases)
            {
                if (workflowCasesModel.StatusName != null)
                {
                    workflowCasesModel.Status =
                        WorkflowCaseStatuses.Statuses.First(y => y.Key == workflowCasesModel.StatusName).Value;
                    if (!string.IsNullOrEmpty(workflowCasesModel.SolvedBy))
                    {
                        workflowCasesModel.ToBeSolvedById =
                            idsSites.First(y => y.Name == workflowCasesModel.SolvedBy).Id;
                    }
                }
            }
        }

        return new OperationDataResult<Paged<WorkflowCasesModel>>(true,
            new Paged<WorkflowCasesModel> { Entities = workflowCases, Total = total });
    }

    public async Task<OperationDataResult<WorkflowCasesUpdateModel>> Read(int id)
    {
        var core = await coreHelper.GetCore();
        var sdkDbContext = core.DbContextHelper.GetDbContext();

        var secondEformIdValue = workflowPnDbContext.PluginConfigurationValues
            .SingleOrDefault(x => x.Name == "WorkflowBaseSettings:SecondEformId")?.Value;
        var secondEformId = int.Parse(secondEformIdValue!);
        var childCheckList = await sdkDbContext.CheckLists.FirstAsync(x => x.ParentId == secondEformId);
        var fieldIdOfTaskCompleted =
            await sdkDbContext.Fields.FirstAsync(x => x.CheckListId == childCheckList.Id && x.FieldTypeId == 5);

        // get query
        var workflowDbCase = await workflowPnDbContext.WorkflowCases
            .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
            .Where(x => x.Id == id).FirstOrDefaultAsync();

        if (workflowDbCase == null)
        {
            return new OperationDataResult<WorkflowCasesUpdateModel>(false,
                workflowLocalizationService.GetString("WorkflowCaseNotFound"));
        }

        var incidentPlaceListId =
            await workflowPnDbContext.PluginConfigurationValues.SingleOrDefaultAsync(x =>
                x.Name == $"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentPlaceListId)}");

        var incidentTypeListId = await workflowPnDbContext.PluginConfigurationValues.SingleOrDefaultAsync(x =>
            x.Name == $"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentTypeListId)}");

        var workflowCase = new WorkflowCasesUpdateModel
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
            Id = workflowDbCase.Id,
            CreatedBySiteName = workflowDbCase.CreatedBySiteName,
            // FieldIdPicturesOfTaskDone = fieldIdOfTaskCompleted.Id, // TODO implement correct frontend part to make this work
            FieldIdPicturesOfTaskDone = 0,

        };

        var picturesOfTasks = await workflowPnDbContext.PicturesOfTasks
            .Where(x => x.WorkflowCaseId == workflowDbCase.Id
                        && x.WorkflowState != Constants.WorkflowStates.Removed).ToListAsync();

        var i = picturesOfTasks.Count;
        foreach (var picturesOfTask in picturesOfTasks)
        {
            var uploadedData = await sdkDbContext.UploadedDatas.SingleOrDefaultAsync(x =>
                x.Id == picturesOfTask.UploadedDataId && x.WorkflowState != Constants.WorkflowStates.Removed);

            if (uploadedData != null)
            {
                var fileName = picturesOfTask.FileName;
                if (fileName.Length < 25)
                {
                    var ud = await sdkDbContext.UploadedDatas.AsNoTracking()
                        .SingleAsync(x => x.Id == picturesOfTask.UploadedDataId);

                    if (ud.FileLocation.Contains("https"))
                    {
                        await core.DownloadUploadedData(ud.Id);
                        ud = await sdkDbContext.UploadedDatas.AsNoTracking()
                            .SingleAsync(x => x.Id == picturesOfTask.UploadedDataId);
                    }

                    fileName = $"{picturesOfTask.UploadedDataId}_700_{ud.Checksum}{ud.Extension}";
                    picturesOfTask.FileName = fileName;
                    await picturesOfTask.Update(workflowPnDbContext);
                }

                var fieldValue = new Infrastructure.Models.FieldValue()
                {
                    Id = picturesOfTask.Id,
                    Longitude = picturesOfTask.Longitude,
                    Latitude = picturesOfTask.Latitude,
                    UploadedDataObj = new UploadedDataObj
                    {
                        Id = uploadedData.Id,
                        FileName = fileName
                    },
                };
                workflowCase.PicturesOfTask.Add(fieldValue);
            }
            else
            {
                await picturesOfTask.Delete(workflowPnDbContext);
            }
        }

        if (i == 0)
        {
            workflowDbCase.PhotosExist = false;
            await workflowDbCase.Update(workflowPnDbContext);
        }

        var picturesOfTaskDones = await workflowPnDbContext.PicturesOfTaskDone
            .Where(x => x.WorkflowCaseId == workflowDbCase.Id
                        && x.WorkflowState != Constants.WorkflowStates.Removed).ToListAsync();

        foreach (var picturesOfTask in picturesOfTaskDones)
        {
            var result = await sdkDbContext.UploadedDatas.SingleOrDefaultAsync(x =>
                x.Id == picturesOfTask.UploadedDataId && x.WorkflowState != Constants.WorkflowStates.Removed);

            if (result != null)
            {
                var fieldValue = new Infrastructure.Models.FieldValue()
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
                if (fieldValue.UploadedDataObj.FileName.Length > 20)
                {
                    workflowCase.PicturesOfTaskDone.Add(fieldValue);
                }
                else
                {
                    await picturesOfTask.Delete(workflowPnDbContext);
                }
            }
            else
            {
                await picturesOfTask.Delete(workflowPnDbContext);
            }
        }

        if (!string.IsNullOrEmpty(workflowDbCase.Status))
        {
            workflowCase.Status = WorkflowCaseStatuses.Statuses.First(y => y.Key == workflowDbCase.Status).Value;
        }

        if (!string.IsNullOrEmpty(workflowDbCase.SolvedBy))
        {
            workflowCase.ToBeSolvedById = sdkDbContext.Sites
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .First(y => y.Name == workflowDbCase.SolvedBy).Id;
        }

        return new OperationDataResult<WorkflowCasesUpdateModel>(true, workflowCase);
    }

    public async Task<OperationResult> UpdateWorkflowCase(WorkflowCasesUpdateModel model)
    {
        var core = await coreHelper.GetCore();
        try
        {
            var solvedByNotSelected = !model.ToBeSolvedById.HasValue;
            var statusClosed = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "Closed").Value;
            var statusOngoing = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "Ongoing").Value;
            var notInitiated = model.Status ==
                               WorkflowCaseStatuses.Statuses.First(x => x.Key == "Not initiated").Value;
            var canceled = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "Canceled").Value;
            var noStatus = model.Status == WorkflowCaseStatuses.Statuses.First(x => x.Key == "No status").Value;

            var workflowCase = await workflowPnDbContext.WorkflowCases.Where(x => x.Id == model.Id)
                .FirstOrDefaultAsync();
            if (workflowCase == null)
            {
                return new OperationResult(false, workflowLocalizationService.GetString("WorkflowCaseNotFound"));
            }

            var sdkDbContext = core.DbContextHelper.GetDbContext();
            var solvedUser = await sdkDbContext.Sites
                .Where(x => x.Id == model.ToBeSolvedById)
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

            workflowCase.UpdatedByUserId = userService.UserId;
            workflowCase.Deadline = string.IsNullOrEmpty(model.Deadline) ? null : DateTime.Parse(model.Deadline);
            workflowCase.IncidentPlace = model.IncidentPlace;
            if (model.IncidentPlaceId != null)
            {
                workflowCase.IncidentPlaceId = (int)model.IncidentPlaceId;
            }

            workflowCase.IncidentType = model.IncidentType;
            if (model.IncidentTypeId != null)
            {
                workflowCase.IncidentTypeId = (int)model.IncidentTypeId;
            }

            workflowCase.ActionPlan = model.ActionPlan;
            workflowCase.Description = model.Description;
            workflowCase.DateOfIncident = DateTime.Parse(model.DateOfIncident);

            await workflowCase.Update(workflowPnDbContext);

            switch (solvedByNotSelected)
            {
                case true when statusClosed:
                {
                    var theCase = await
                        sdkDbContext.Cases.SingleOrDefaultAsync(x =>
                            x.MicrotingCheckUid == workflowCase.CheckMicrotingUid);
                    var createdBySite = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.Id == theCase.SiteId);
                    if (!string.IsNullOrEmpty(workflowCase.SolvedBy))
                    {
                        await GenerateReportAndSendEmail(createdBySite.LanguageId, createdBySite.Name.Replace(" ", ""), workflowCase, "");
                    }

                    if (workflowCase.DeployedMicrotingUid != null)
                    {
                        await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                    }

                    workflowCase.DeployedMicrotingUid = null;
                    await workflowCase.Update(workflowPnDbContext);
                }
                    break;
                case false when canceled:
                case false when noStatus:
                {
                    if (workflowCase.DeployedMicrotingUid != null)
                    {
                        await core.CaseDelete((int) workflowCase.DeployedMicrotingUid);
                    }

                    workflowCase.DeployedMicrotingUid = null;
                    await workflowCase.Update(workflowPnDbContext);


                    await workflowCase.Update(workflowPnDbContext);
                    var _case = await
                        sdkDbContext.Cases.SingleOrDefaultAsync(x =>
                            x.MicrotingCheckUid == workflowCase.CheckMicrotingUid);

                    var createdBySite = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.Id == _case.SiteId);

                    await GenerateReportAndSendEmail(createdBySite.LanguageId, createdBySite.Name.Replace(" ", ""), workflowCase, workflowCase.SolvedBy);


                    break;
                }
                case false when statusClosed:
                {
                    var theCase = await
                        sdkDbContext.Cases.SingleOrDefaultAsync(x =>
                            x.MicrotingCheckUid == workflowCase.CheckMicrotingUid);
                    var createdBySite = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.Id == theCase.SiteId);
                    if (!string.IsNullOrEmpty(workflowCase.SolvedBy))
                    {
                        await GenerateReportAndSendEmail(createdBySite.LanguageId, createdBySite.Name.Replace(" ", ""), workflowCase, workflowCase.SolvedBy);
                    }

                    if (workflowCase.DeployedMicrotingUid != null)
                    {
                        await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                    }

                    workflowCase.DeployedMicrotingUid = null;
                    await workflowCase.Update(workflowPnDbContext);
                    break;
                }
                case false when statusOngoing:
                case false when notInitiated:
                {

                    if (workflowCase.Deadline == null)
                    {
                        return new OperationResult(false,
                            workflowLocalizationService.GetString("DeadlineIsMissing"));
                    }

                    var picturesOfTasks = new List<string>();

                    var pictures =
                        await workflowPnDbContext.PicturesOfTasks.Where(x =>
                            x.WorkflowCaseId == workflowCase.Id).ToListAsync();

                    foreach (var picturesOfTask in pictures)
                    {
                        picturesOfTasks.Add(picturesOfTask.FileName);
                    }

                    var hash = await GeneratePdf(picturesOfTasks, (int)model.ToBeSolvedById);

                    // eform is deployed to solver device user
                    var folder =
                        await sdkDbContext.Folders.SingleOrDefaultAsync(x =>
                            x.Id == options.Value.FolderTasksId);
                    var mainElement = await core.ReadeForm(options.Value.SecondEformId,
                        await userService.GetCurrentUserLanguage());
                    mainElement.Repeated = 1;
                    mainElement.EndDate = DateTime.Now.AddYears(10).ToUniversalTime();
                    mainElement.StartDate = DateTime.Now.ToUniversalTime();
                    mainElement.CheckListFolderName = folder.MicrotingUid.ToString();
                    mainElement.PushMessageTitle = workflowCase.IncidentType;

                    var dataElement = mainElement.ElementList[0] as DataElement;
                    mainElement.Label = workflowCase.IncidentType;
                    dataElement.Label = workflowCase.IncidentType;
                    var startDate = new DateTime(2020, 1, 1);
                    mainElement.DisplayOrder = (workflowCase.Deadline - startDate).Value.Days;
                    dataElement.Description = new CDataValue
                    {
                        InderValue =
                            $"{workflowCase.IncidentPlace}<br><strong>Deadline:</strong> {workflowCase.Deadline?.ToString("dd.MM.yyyy")}" // Deadline
                    };
                    var info = dataElement!.DataItemList[0];
                    mainElement.PushMessageBody =
                        $"{workflowCase.IncidentPlace}\nDeadline: {workflowCase.Deadline?.ToString("dd.MM.yyyy")}"; // Deadline

                    info.Label =
                        $@"<strong>Oprettet af:</strong> {workflowCase.CreatedBySiteName}<br>"; // Event created by
                    info.Label +=
                        $@"<strong>Dato:</strong> {workflowCase.DateOfIncident:dd.MM.yyyy}<br>"; // Event date
                    info.Label += $@"<strong>Type:</strong> {workflowCase.IncidentType}<br>"; // Event
                    info.Label += $@"<strong>Sted:</strong> {workflowCase.IncidentPlace}<br>"; // Event location
                    info.Label +=
                        $@"<strong>Deadline:</strong> {workflowCase.Deadline?.ToString("dd.MM.yyyy")}<br>"; // Deadline
                    info.Label +=
                        $@"<strong>Status:</strong> {GetStatusTranslated(workflowCase.Status)}<br>"; // Status

                    dataElement!.DataItemList[0] = info;
                    mainElement.ElementList[0] = dataElement;

                    if (hash != null)
                    {
                        ((ShowPdf)dataElement.DataItemList[1]).Value = hash;
                    }

                    ((Comment)dataElement.DataItemList[2]).Value = workflowCase.Description;

                    ((Comment)dataElement.DataItemList[3]).Value = workflowCase.ActionPlan;

                    if (workflowCase.DeployedMicrotingUid != null)
                    {
                        await core.CaseDelete((int)workflowCase.DeployedMicrotingUid);
                    }

                    var site = await sdkDbContext.Sites.SingleOrDefaultAsync(x =>
                        x.Id == model.ToBeSolvedById);
                    var theCase = await
                        sdkDbContext.Cases.SingleOrDefaultAsync(x =>
                            x.MicrotingCheckUid == workflowCase.CheckMicrotingUid);

                    workflowCase.DeployedMicrotingUid =
                        (int)await core.CaseCreate(mainElement, "", (int)site.MicrotingUid, folder.Id);
                    await workflowCase.Update(workflowPnDbContext);
                    var createdBySite = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.Id == theCase.SiteId);

                    await GenerateReportAndSendEmail(createdBySite.LanguageId, createdBySite.Name.Replace(" ", ""), workflowCase, workflowCase.SolvedBy);

                    break;
                }
            }

            return new OperationResult(true, workflowLocalizationService.GetString("CaseHasBeenUpdated"));
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            logger.LogError(ex.Message);
            logger.LogTrace(ex.StackTrace);
            return new OperationResult(false,
                $"{workflowLocalizationService.GetString("CaseCouldNotBeUpdated")} Exception: {ex.Message}");
        }
    }

    private async Task<string> GeneratePdf(List<string> picturesOfTasks, int sitId)
    {
        var core = await coreHelper.GetCore();
        picturesOfTasks.Reverse();
        var downloadPath = Path.Combine(Path.GetTempPath(), "reports", "results");
        Directory.CreateDirectory(downloadPath);
        var timeStamp = DateTime.UtcNow.ToString("yyyyMMdd") + "_" + DateTime.UtcNow.ToString("hhmmss");
        var tempPDFFileName = $"{timeStamp}{sitId}_temp.pdf";
        var tempPDFFilePath = Path.Combine(downloadPath, tempPDFFileName);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Content()
                    .Padding(1, QuestPDF.Infrastructure.Unit.Centimetre)
                    .Column(x =>
                    {
                        // loop over all images and add them to the document
                        var i = 0;
                        foreach (var imageName in picturesOfTasks)
                        {
                            var storageResult = core.GetFileFromS3Storage(imageName).GetAwaiter().GetResult();
                            x.Item().Image(storageResult.ResponseStream)
                                .FitArea();
                            if (i < picturesOfTasks.Count - 1)
                            {
                                x.Item().PageBreak();
                            }

                            i++;
                        }
                    });
            });
        }).GeneratePdf();

        await using var fileStream = new FileStream(tempPDFFilePath, FileMode.Create, FileAccess.Write);
        // save the byte[] to a file.
        await fileStream.WriteAsync(document, 0, document.Length);
        await fileStream.FlushAsync();

        // Upload PDF
        // string pdfFileName = null;
        var hash = await core.PdfUpload(tempPDFFilePath);
        if (hash != null)
        {
            //rename local file
            var fileInfo = new FileInfo(tempPDFFilePath);
            fileInfo.CopyTo(downloadPath + "/" + hash + ".pdf", true);
            fileInfo.Delete();
            await core.PutFileToStorageSystem(Path.Combine(downloadPath, $"{hash}.pdf"), $"{hash}.pdf");

            // delete local file
            File.Delete(downloadPath + "/" + hash + ".pdf");
            // TODO Remove from file storage?
        }

        return hash;
    }

    public async Task<OperationDataResult<List<WorkflowPlacesModel>>> GetPlaces()
    {
        try
        {
            if (options.Value.FirstEformId != 0)
            {
                var core = await coreHelper.GetCore();
                var sdkDbContext = core.DbContextHelper.GetDbContext();

                var fieldWithPlaces = await sdkDbContext.Fields
                    .Where(x => x.CheckListId == options.Value.FirstEformId)
                    .Where(x => x.FieldTypeId == 8) // fieldType.id == 8; fieldType.type -> SingleSelect
                    .Select(x => x.Id)
                    .Skip(1)
                    .FirstOrDefaultAsync();

                var languageId = await userService.GetCurrentUserLanguage();

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
            SentrySdk.CaptureException(ex);
            logger.LogError(ex.Message);
            logger.LogTrace(ex.StackTrace);
            return new OperationDataResult<List<WorkflowPlacesModel>>(false,
                $"{workflowLocalizationService.GetString("ErrorWhileGetPlaces")} Exception: {ex.Message}");
        }
    }

    public async Task<OperationResult> Delete(int id)
    {

        var core = await coreHelper.GetCore();
        var workflowDbCase = await workflowPnDbContext.WorkflowCases
            .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
            .Where(x => x.Id == id).FirstOrDefaultAsync();

        if (workflowDbCase == null)
        {
            return new OperationDataResult<WorkflowCasesUpdateModel>(false,
                workflowLocalizationService.GetString("WorkflowCaseNotFound"));
        }

        if (workflowDbCase.DeployedMicrotingUid != null)
        {
            await core.CaseDelete((int)workflowDbCase.DeployedMicrotingUid);
        }

        workflowDbCase.DeployedMicrotingUid = null;

        await workflowDbCase.Update(workflowPnDbContext);
        await workflowDbCase.Delete(workflowPnDbContext);

        return new OperationDataResult<WorkflowCasesUpdateModel>(true);

    }

    public async Task<OperationDataResult<Stream>> DownloadEFormPdf(int caseId, string fileType)
    {
        var workflowDbCase = await workflowPnDbContext.WorkflowCases
            .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
            .Where(x => x.Id == caseId).FirstOrDefaultAsync();

        if (workflowDbCase == null)
        {
            return new OperationDataResult<Stream>(
                false,
                "ErrorWhileGeneratingReportFile");
        }

        var core = await coreHelper.GetCore();
        var sdkDbContext = core.DbContextHelper.GetDbContext();
        var reportHelper = new WorkflowReportHelper(core, workflowPnDbContext);

        var filePath = await reportHelper.GenerateReportAnd(0, workflowDbCase, fileType);

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return new OperationDataResult<Stream>(true, fileStream);
    }

    public async Task<FileStream> DownloadCasesAsXlsx()
    {
        // Ensure the results directory exists
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "results"));

        // Generate a temporary file name for the Excel file
        var fileName = Path.Combine(Path.GetTempPath(), "results", $"{Guid.NewGuid()}.xlsx");

        // Retrieve the cases data
        var result = await Index(new WorkflowCasesResponse
        {
            PageSize = 1000000,
            Sort = "Id",
            IsSortDsc = false
        });

        // Create the Excel file using OpenXml
        using (var spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
        {
            // Add WorkbookPart to the document
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets collection to the workbook
            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            // Create the Sheet and append it to the Sheets collection
            var sheet = new Sheet
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Workflow cases"
            };
            sheets.Append(sheet);

            // Get the SheetData from the worksheet
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Create the header row
            var headerRow = new Row();
            headerRow.Append(
                ConstructCell(workflowLocalizationService.GetString("Id"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("DateOfIncident"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("CreatedBy"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("IncidentType"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("IncidentPlace"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("Description"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("Deadline"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("ActionPlan"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("SolvedBy"), CellValues.String),
                ConstructCell(workflowLocalizationService.GetString("Status"), CellValues.String)
            );
            sheetData.Append(headerRow);

            // Populate the data rows
            for (var i = 0; i < result.Model.Entities.Count; i++)
            {
                var item = result.Model.Entities[i];
                var status = WorkflowCaseStatuses.Statuses.First(x => x.Value == item.Status).Key;

                var dataRow = new Row();
                dataRow.Append(
                    ConstructCell(item.Id.ToString(), CellValues.Number),
                    ConstructCell(item.DateOfIncident.ToString("yyyy-MM-dd"), CellValues.String),
                    ConstructCell(item.CreatedBySiteName, CellValues.String),
                    ConstructCell(item.IncidentType, CellValues.String),
                    ConstructCell(item.IncidentPlace, CellValues.String),
                    ConstructCell(item.Description, CellValues.String),
                    ConstructCell(item.Deadline?.ToString("yyyy-MM-dd"), CellValues.String),
                    ConstructCell(item.ActionPlan, CellValues.String),
                    ConstructCell(item.SolvedBy, CellValues.String),
                    ConstructCell(GetStatusTranslated(status), CellValues.String)
                );
                sheetData.Append(dataRow);
            }

            // Save the workbook
            workbookPart.Workbook.Save();
        }

        // Return the FileStream for the generated Excel file
        return new FileStream(fileName, FileMode.Open, FileAccess.Read);
    }

// Utility method for constructing OpenXml cells
    private static Cell ConstructCell(string value, CellValues dataType) =>
        new Cell
        {
            CellValue = new CellValue(value),
            DataType = new EnumValue<CellValues>(dataType)
        };


    private async Task<string> InsertImage(Core core, string imageName, string itemsHtml, int imageSize,
        int imageWidth, string basePicturePath)
    {

        var s3Enabled = core.GetSdkSetting(Settings.s3Enabled).Result.ToLower() == "true";
        var swiftEnabled = core.GetSdkSetting(Settings.swiftEnabled).Result.ToLower() == "true";
        var filePath = Path.Combine(basePicturePath, imageName);
        Stream stream;
        if (s3Enabled)
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
            var base64String = image.ToBase64();
            itemsHtml +=
                $@"<p><img src=""data:image/png;base64,{base64String}"" width=""{imageWidth}px"" alt="""" /></p>";
        }

        await stream.DisposeAsync();

        return itemsHtml;
    }

    private string GetStatusTranslated(string constant)
    {
        switch (constant)
        {
            case "Not initiated":
                return "Ikke igangsat";
            case "Ongoing":
                return "Igangværende";
            case "No status":
                return "Vælg status";
            case "Closed":
                return "Afsluttet";
            case "Canceled":
                return "Annulleret";
            default:
                return "Ikke igangsat";
        }
    }

    private async Task SendFileAsync(
        string fromEmail,
        string fromName,
        string subject,
        string to,
        string fileName,
        string text = null, string html = null)
    {
        try
        {
            var sendGridKey =
                _baseDbContext.ConfigurationValues.Single(x => x.Id == "EmailSettings:SendGridKey");
            var client = new SendGridClient(sendGridKey.Value);
            var fromEmailAddress = new EmailAddress(fromEmail.Replace(" ", ""), fromName);
            var toEmail = new EmailAddress(to.Replace(" ", ""));
            var msg = MailHelper.CreateSingleEmail(fromEmailAddress, toEmail, subject, text, html);
            var bytes = await File.ReadAllBytesAsync(fileName);
            var file = Convert.ToBase64String(bytes);
            msg.AddAttachment(Path.GetFileName(fileName), file);
            var response = await client.SendEmailAsync(msg);
            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
            {
                throw new Exception($"Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            throw new Exception("Failed to send email message", ex);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    public async Task GenerateReportAndSendEmail(int languageId, string userName, WorkflowCase workflowCase, string solvedBy)
    {
        var emailRecipient = await _baseDbContext.EmailRecipients.SingleOrDefaultAsync(x => x.Name.Replace(" ", "") ==
            userName
                .Replace("Mobil", "")
                .Replace("Tablet", ""));
        var _workflowReportHelper =
            new WorkflowReportHelper(await coreHelper.GetCore(), workflowPnDbContext);
        var filePath = await _workflowReportHelper.GenerateReportAnd(languageId, workflowCase, "pdf");
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name;

        var stream = assembly.GetManifestResourceStream($"{assemblyName}.Resources.Email.html");
        string html;
        if (stream == null)
        {
            throw new InvalidOperationException("Resource not found");
        }

        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            html = await reader.ReadToEndAsync();
        }

        html = html
            .Replace(
                "<a href=\"{{link}}\">Link til sag</a>", "")
            .Replace("{{CreatedBy}}", workflowCase.CreatedBySiteName)
            .Replace("{{CreatedAt}}", workflowCase.DateOfIncident.ToString("dd-MM-yyyy"))
            .Replace("{{Type}}", workflowCase.IncidentType)
            .Replace("{{Location}}", workflowCase.IncidentPlace)
            .Replace("{{Description}}", workflowCase.Description.Replace("&", "&amp;"))
            .Replace("{{SolvedBy}}", solvedBy)
            .Replace("{{ActionPlan}}", "");

        await SendFileAsync(
            "no-reply@microting.com",
            userName,
            $"Opfølgning: {workflowCase.IncidentType};  {workflowCase.IncidentPlace}; {workflowCase.CreatedAt:dd-MM-yyyy}",
            emailRecipient?.Email,
            filePath,
            null,
            html);
    }
}
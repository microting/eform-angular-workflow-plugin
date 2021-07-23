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
    using Messages;
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
            var query = _workflowPnDbContext.WorkflowCases
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Where(x => x.Id == id);

            //get from db
            var workflowCase = await query
                .Select(x => new WorkflowCasesUpdateModel
                {
                    ActionPlan = x.ActionPlan,
                    DateOfIncident = x.DateOfIncident,
                    Deadline = x.Deadline,
                    Description = x.Description,
                    Id = x.Id,
                    //it comment because it posible System.InvalidOperationException: The LINQ expression 'y' could not be translated.
                    //Status = WorkflowCaseStatuses.Statuses.FirstOrDefault(y => y.Key == x.Status).Value,
                    //ToBeSolvedById = idsSites.FirstOrDefault(y => y.Name == x.SolvedBy).Id,
                })
                .FirstOrDefaultAsync();


            if (workflowCase == null)
            {
                return new OperationDataResult<WorkflowCasesUpdateModel>(false, _workflowLocalizationService.GetString("WorkflowCaseNotFound"));
            }

            var statusName = await query.Select(x => x.Status).FirstAsync();
            var toBeSolvedBy = await query.Select(x => x.SolvedBy).FirstAsync();
            var incidentPlace = await query.Select(x => x.IncidentPlace).FirstAsync();
            if (!string.IsNullOrEmpty(statusName))
            {
                workflowCase.Status = WorkflowCaseStatuses.Statuses.First(y => y.Key == statusName).Value;
            }

            if (!string.IsNullOrEmpty(toBeSolvedBy))
            {
                workflowCase.ToBeSolvedById = sdkDbContext.Sites.First(y => y.Name == toBeSolvedBy).Id;
            }

            if (_options.Value.FirstEformId != 0/*if the form is installed*/ && !string.IsNullOrEmpty(incidentPlace))
            {
                var fieldWithPlaces = await sdkDbContext.Fields
                    //.Where(x => x.CheckListId == _options.Value.FirstEformId)
                    //.Where(x => x.FieldTypeId == 8) // fieldType.id == 8; fieldType.type -> SingleSelect
                    .Where(x => x.OriginalId == 374097.ToString())
                    .Select(x => x.Id)
                    //.Skip(1)
                    .FirstOrDefaultAsync();

                var languageId = await _userService.GetCurrentUserLanguage();

                workflowCase.IncidentPlace = await sdkDbContext.FieldOptions
                    .Where(x => x.FieldId == fieldWithPlaces)
                    .Include(x => x.FieldOptionTranslations)
                    .SelectMany(x => x.FieldOptionTranslations)
                    .Where(x => x.LanguageId == languageId.Id)
                    .Where(x => x.Text == incidentPlace)
                    .Select(x => x.Id )
                    .FirstOrDefaultAsync();
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
                workflowCase.Deadline = model.Deadline;
                workflowCase.IncidentPlace = model.IncidentPlace.ToString();
                //if(model.IncidentPlace.HasValue)
                // {
                //     workflowCase.IncidentPlace = await sdkDbContext.FieldOptionTranslations
                //         .Where(x => x.Id == model.IncidentPlace)
                //         .Select(x => x.Text)
                //         .FirstAsync();
                // }
                workflowCase.ActionPlan = model.ActionPlan;
                workflowCase.Description = model.Description;
                workflowCase.DateOfIncident = model.DateOfIncident;

                await workflowCase.Update(_workflowPnDbContext);

                switch (solvedByNotSelected)
                {
                    case true when statusClosed:
                        // send email with pdf report to device user
                        await _bus.SendLocal(new QueueEformEmail
                        {
                            CaseId = model.Id,
                            UserName = await _userService.GetCurrentUserFullName(),
                            CurrentUserLanguage = await _userService.GetCurrentUserLanguage(),
                            SolvedUser = new List<KeyValuePair<string, Language>>()
                        });
                        break;
                    case false when statusClosed:
                        {
                            var language = await sdkDbContext.Languages.FirstAsync(x => x.Id == solvedUser.LanguageId);
                            var solvedUsers = new List<KeyValuePair<string, Language>>
                            {
                                new(solvedUser.Name, language),
                            };

                            // send email with pdf report to device user and solved user
                            await _bus.SendLocal(new QueueEformEmail
                            {
                                CaseId = model.Id,
                                UserName = await _userService.GetCurrentUserFullName(),
                                CurrentUserLanguage = await _userService.GetCurrentUserLanguage(),
                                SolvedUser = solvedUsers
                            });
                            break;
                        }
                    case false when statusOngoing:
                        {
                            // eform is deployed to solver device user
                            var mainElement = await core.ReadeForm(_options.Value.SecondEformId, await _userService.GetCurrentUserLanguage());
                            mainElement.Repeated = 1;
                            mainElement.EndDate = DateTime.Now.AddYears(10).ToUniversalTime();
                            mainElement.StartDate = DateTime.Now.ToUniversalTime();

                            var checkListValue = mainElement.ElementList[0] as CheckListValue;
                            var info = checkListValue!.DataItemList[0];

                            info.Label = $@"INFO<br><strong>Event created by: {workflowCase.SolvedBy}</strong><br>";
                            info.Label += $@"<strong>Event date: {workflowCase.DateOfIncident}</strong><br>";
                            info.Label += $@"<strong>Event: {workflowCase.IncidentType}</strong><br>";
                            info.Label += $@"<strong>Event location: {workflowCase.IncidentPlace}</strong><br>";
                            info.Label += $@"<strong>Deadline: {workflowCase.Deadline}</strong><br>";
                            info.Label += $@"<strong>Status: {workflowCase.Status}</strong><br>";

                            checkListValue!.DataItemList[0] = info;
                            mainElement.ElementList[0] = checkListValue;

                            await core.CaseCreate(mainElement, "", (int)model.ToBeSolvedById, 0);
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
    }
}

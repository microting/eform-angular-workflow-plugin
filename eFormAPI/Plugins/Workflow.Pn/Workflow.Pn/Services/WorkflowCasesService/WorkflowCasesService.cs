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
    using System.Linq;
    using System.Threading.Tasks;
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

    public class WorkflowCasesService: IWorkflowCasesService
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
                query = query.Where(x =>
                    x.Id.ToString().Contains(request.NameFilter) ||
                    x.IncidentType.Contains(request.NameFilter) ||
                    x.ActionPlan.Contains(request.NameFilter) ||
                    x.Description.Contains(request.NameFilter) ||
                    x.IncidentPlace.Contains(request.NameFilter) ||
                    x.SolvedBy.Contains(request.NameFilter)
                    );
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
                    workflowCasesModel.Status =
                        WorkflowCaseStatuses.Statuses.First(y => y.Key == workflowCasesModel.StatusName).Value;
                    if(!string.IsNullOrEmpty(workflowCasesModel.ToBeSolvedBy))
                    {
                        workflowCasesModel.ToBeSolvedById =
                            idsSites.First(y => y.Name == workflowCasesModel.ToBeSolvedBy).Id;
                    }
                }
            }
            return new OperationDataResult<Paged<WorkflowCasesModel>>(true, new Paged<WorkflowCasesModel> { Entities = workflowCases, Total = total });

        }

        public async Task<OperationResult> UpdateWorkflowCase(WorkflowCasesUpdateModel model)
        {
            var core = await _coreHelper.GetCore();
            try
            {
                var solvedByNotSelected = model.ToBeSolvedById == 0;
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
                    .FirstAsync();

                var language = await sdkDbContext.Languages.FirstAsync(x => x.Id == solvedUser.LanguageId);
                workflowCase.SolvedBy = solvedUser.Name;
                workflowCase.Status = WorkflowCaseStatuses.Statuses.First(x => x.Value == model.Status).Key;
                workflowCase.UpdatedByUserId = _userService.UserId;
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

                            await core.CaseCreate(mainElement, "", model.ToBeSolvedById, 0);
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
    }
}

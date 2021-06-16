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
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microting.eForm.Infrastructure.Constants;
    using Microting.eForm.Infrastructure.Data.Entities;
    using Microting.eFormApi.BasePn.Abstractions;
    using Microting.eFormApi.BasePn.Infrastructure.Delegates.CaseUpdate;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Application.Case.CaseEdit;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Common;
    using Microting.eFormWorkflowBase.Infrastructure.Data;
    using Rebus.Bus;
    using WorkflowLocalizationService;

    public class WorkflowCasesService: IWorkflowCasesService
    {
        private readonly IEFormCoreService _coreHelper;
        private readonly IWorkflowLocalizationService _workflowLocalizationService;
        private readonly IUserService _userService;
        private readonly IBus _bus;
        private readonly WorkflowPnDbContext _workflowPnDbContext;

        public WorkflowCasesService(IEFormCoreService coreHelper,
            IUserService userService,
            IBus bus,
            WorkflowPnDbContext workflowPnDbContext,
            IWorkflowLocalizationService workflowLocalizationService)
        {
            _coreHelper = coreHelper;
            _userService = userService;
            _workflowLocalizationService = workflowLocalizationService;
            _workflowPnDbContext = workflowPnDbContext;
            _bus = bus;
        }

        public async Task<OperationDataResult<Paged<WorkflowCasesModel>>> Index(WorkflowCasesResponse response)
        {
            // get query
            var query = _workflowPnDbContext.WorkflowCases
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed);

            // add filtering

            if (!string.IsNullOrEmpty(response.NameFilter))
            {
                query = query.Where(x =>
                    x.Id.ToString().Contains(response.NameFilter) ||
                    x.IncedentType.Contains(response.NameFilter) ||
                    x.ActionPlan.Contains(response.NameFilter) ||
                    x.Description.Contains(response.NameFilter) ||
                    x.IncedentPlace.Contains(response.NameFilter) ||
                    x.SolvedBy.Contains(response.NameFilter)
                    );
            }

            // add sorting
            query = QueryHelper.AddSortToQuery(query, response.Sort, response.IsSortDsc);

            var total = await query.Select(x => x.Id).CountAsync();

            // add select to query and get from db
            var workflowCases = await query
                .Select(x => new WorkflowCasesModel
                {
                    ActionPlan = x.ActionPlan,
                    DateOfIncident = x.DateOfIncedent,
                    Deadline = x.Deadline,
                    Description = x.Description,
                    Id = x.Id,
                    IncidentPlace = x.IncedentPlace,
                    IncidentType = x.IncedentType,
                    PhotosExist = x.Photos.Any(),
                    Status = x.Status,
                    ToBeSolvedBy = x.SolvedBy,
                    UpdatedAt = (DateTime)x.UpdatedAt,
                })
                .ToListAsync();
            return new OperationDataResult<Paged<WorkflowCasesModel>>(true, new Paged<WorkflowCasesModel>{Entities = workflowCases, Total = total});

        }

        public async Task<OperationResult> UpdateCase(ReplyRequest model)
        {

            var checkListValueList = new List<string>();
            var fieldValueList = new List<string>();
            var core = await _coreHelper.GetCore();
            var currentUserLanguage = await _userService.GetCurrentUserLanguage();
            try
            {
                model.ElementList.ForEach(element =>
                {
                    checkListValueList.AddRange(CaseUpdateHelper.GetCheckList(element));
                    fieldValueList.AddRange(CaseUpdateHelper.GetFieldList(element));
                });
            }
            catch (Exception ex)
            {
                Log.LogException(ex.Message);
                Log.LogException(ex.StackTrace);
                return new OperationResult(false, $"{_workflowLocalizationService.GetString("CaseCouldNotBeUpdated")} Exception: {ex.Message}");
            }

            try
            {
                await core.CaseUpdate(model.Id, fieldValueList, checkListValueList);
                await core.CaseUpdateFieldValues(model.Id, currentUserLanguage);

                if (CaseUpdateDelegates.CaseUpdateDelegate != null)
                {
                    var invocationList = CaseUpdateDelegates.CaseUpdateDelegate
                        .GetInvocationList();
                    foreach (var func in invocationList)
                    {
                        func.DynamicInvoke(model.Id);
                    }
                }

                var solvedByNotSelected = false;
                var statusClosed = false;
                var statusOngoing = false;
                var solvedUserNames = new List<string>();

                foreach (var editRequest in model.ElementList)
                {
                    var fieldSolvedBy = editRequest.Fields.FirstOrDefault(x => x.FieldType == "Solved by");
                    var fieldStatus = editRequest.Fields.FirstOrDefault(x => x.FieldType == "Status");
                    if(fieldSolvedBy != null && fieldStatus != null)
                    {
                        solvedByNotSelected = fieldSolvedBy.FieldValues.Any(x => x.Value == "Not selected");
                        solvedUserNames = fieldSolvedBy.FieldValues.Where(x => x.Value != "Not selected").Select(x => (string)x.Value).ToList(); 
                        statusClosed = fieldStatus.FieldValues.Any(x => x.Value == "Closed");
                        statusOngoing = fieldStatus.FieldValues.Any(x => x.Value == "Ongoing");
                    }
                }

                if (solvedByNotSelected && statusClosed)
                {
                    // send email with pdf report to device user
                    await _bus.SendLocal(new QueueEformEmail
                    {
                        CaseId = model.Id,
                        UserName = await _userService.GetCurrentUserFullName(),
                        CurrentUserLanguage = await _userService.GetCurrentUserLanguage(),
                        SolvedUser = new List<KeyValuePair<string, Language>>()
                    });
                }

                if (!solvedByNotSelected && statusClosed)
                {
                    var solvedUser = new List<KeyValuePair<string, Language>>();
                    var sdkDbContext = core.DbContextHelper.GetDbContext();
                    foreach (var solvedUserName in solvedUserNames)
                    {
                        var user = await _userService.GetByUsernameAsync(solvedUserName);
                        var language = await sdkDbContext.Languages.FirstAsync(x => x.LanguageCode == user.Locale);
                        solvedUser.Add(new KeyValuePair<string, Language>(solvedUserName, language));
                    }
                    // send email with pdf report to device user and solved user
                    await _bus.SendLocal(new QueueEformEmail
                    {
                        CaseId = model.Id,
                        UserName = await _userService.GetCurrentUserFullName(),
                        CurrentUserLanguage = await _userService.GetCurrentUserLanguage(),
                        SolvedUser = solvedUser
                    });
                }

                if (!solvedByNotSelected && statusOngoing)
                {
                    // eform is deployed to solver device user
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

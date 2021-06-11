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
    using Messages;
    using Microting.eFormApi.BasePn.Abstractions;
    using Microting.eFormApi.BasePn.Infrastructure.Delegates.CaseUpdate;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Application.Case.CaseEdit;
    using Rebus.Bus;
    using WorkflowLocalizationService;

    public class WorkflowCasesService: IWorkflowCasesService
    {
        private readonly IEFormCoreService _coreHelper;
        private readonly IWorkflowLocalizationService _workflowLocalizationService;
        private readonly IUserService _userService;
        private readonly IBus _bus;

        public WorkflowCasesService(IEFormCoreService coreHelper,
            IUserService userService,
            IBus bus,
            IWorkflowLocalizationService workflowLocalizationService)
        {
            _coreHelper = coreHelper;
            _userService = userService;
            _workflowLocalizationService = workflowLocalizationService;
            _bus = bus;
        }

        public async Task<OperationResult> UpdateCase(ReplyRequest model)
        {

            var checkListValueList = new List<string>();
            var fieldValueList = new List<string>();
            var core = await _coreHelper.GetCore();
            var language = await _userService.GetCurrentUserLanguage();
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
                await core.CaseUpdateFieldValues(model.Id, language);

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
                var solvedUsers = new List<string>();

                foreach (var editRequest in model.ElementList)
                {
                    var fieldSolvedBy = editRequest.Fields.FirstOrDefault(x => x.FieldType == "Solved by");
                    var fieldStatus = editRequest.Fields.FirstOrDefault(x => x.FieldType == "Status");
                    if(fieldSolvedBy != null && fieldStatus != null)
                    {
                        solvedByNotSelected = fieldSolvedBy.FieldValues.Any(x => x.Value == "Not selected");
                        solvedUsers = fieldSolvedBy.FieldValues.Where(x => x.Value != "Not selected").Select(x => (string)x.Value).ToList(); 
                        statusClosed = fieldStatus.FieldValues.Any(x => x.Value == "Closed");
                        statusOngoing = fieldStatus.FieldValues.Any(x => x.Value == "Ongoing");
                    }
                }

                if (solvedByNotSelected && statusClosed)
                {// send email with pdf report to device user
                    await _bus.SendLocal(new QueueEformEmail
                    { CaseId = model.Id, UserId = _userService.UserId, ListSolvedUser = solvedUsers });
                }

                if (!solvedByNotSelected && statusClosed)
                {
                    // send email with pdf report to device user and solved user
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
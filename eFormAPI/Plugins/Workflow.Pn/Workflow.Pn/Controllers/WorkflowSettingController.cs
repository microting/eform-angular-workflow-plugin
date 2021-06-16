﻿/*
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

namespace Workflow.Pn.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microting.eFormApi.BasePn.Infrastructure.Database.Entities;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using System.Threading.Tasks;
    using Infrastructure.Models.Settings;
    using Microting.eForm.Dto;
    using Services.WorkflowPnSettingsService;

    public class WorkflowSettingController : Controller
    {
        private readonly IWorkflowPnSettingsService _workflowPnSettingsService;

        public WorkflowSettingController(IWorkflowPnSettingsService workflowPnSettingsService)
        {
            _workflowPnSettingsService = workflowPnSettingsService;
        }

        [HttpGet]
        [Authorize(Roles = EformRole.Admin)]
        [Route("api/workflow-pn/settings")]
        public async Task<OperationDataResult<WorkflowSettingsModel>> GetSettings()
        {
            return await _workflowPnSettingsService.GetSettings();
        }

        [HttpPost]
        [Authorize(Roles = EformRole.Admin)]
        [Route("api/workflow-pn/settings")]
        public async Task<OperationResult> UpdateSetting([FromBody] WorkflowSettingsModel workflowSettingsModel)
        {
            return await _workflowPnSettingsService.UpdateSetting(workflowSettingsModel);
        }

        [HttpGet]
        //[Authorize(Roles = EformRole.Admin)]
        [Route("api/workflow-pn/settings/template")]
        public async Task<OperationDataResult<Template_Dto>> UpdateFolderId()
        {
            return await _workflowPnSettingsService.GetTemplate();
        }
    }
}
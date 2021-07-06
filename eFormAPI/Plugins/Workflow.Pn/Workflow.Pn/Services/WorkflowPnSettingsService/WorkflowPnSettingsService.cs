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

namespace Workflow.Pn.Services.WorkflowPnSettingsService
{
    using Infrastructure.Models.Settings;
    using Microsoft.Extensions.Logging;
    using Microting.eFormApi.BasePn.Abstractions;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microting.eForm.Dto;
    using Microting.eFormWorkflowBase.Infrastructure.Data;
    using WorkflowLocalizationService;

    public class WorkflowPnSettingsService : IWorkflowPnSettingsService
    {
        private readonly ILogger<WorkflowPnSettingsService> _logger;
        private readonly IWorkflowLocalizationService _workflowLocalizationService;
        private readonly WorkflowPnDbContext _dbContext;
        private readonly IPluginDbOptions<WorkflowBaseSettings> _options;
        private readonly IUserService _userService;
        private readonly IEFormCoreService _coreHelper;


        public WorkflowPnSettingsService(
            IEFormCoreService coreHelper, 
            ILogger<WorkflowPnSettingsService> logger,
            IWorkflowLocalizationService workflowLocalizationService,
            WorkflowPnDbContext dbContext,
            IPluginDbOptions<WorkflowBaseSettings> options,
            IUserService userService)
        {
            _coreHelper = coreHelper;
            _logger = logger;
            _dbContext = dbContext;
            _options = options;
            _userService = userService;
            _workflowLocalizationService = workflowLocalizationService;
        }

        public async Task<OperationDataResult<WorkflowSettingsModel>> GetSettings()
        {
            try
            {
                var option = _options.Value;

                var settings = new WorkflowSettingsModel
                {
                    FirstEformId = option.FirstEformId == 0 ? (int?) null : option.FirstEformId,
                    SecondEformId = option.SecondEformId == 0 ? (int?) null : option.SecondEformId,
                };

                return new OperationDataResult<WorkflowSettingsModel>(true, settings);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationDataResult<WorkflowSettingsModel>(false,
                    _workflowLocalizationService.GetString("ErrorWhileGetSettings"));
            }
        }

        public async Task<OperationResult> UpdateSetting(WorkflowSettingsModel workflowSettingsModel)
        {
            try
            {
                if (workflowSettingsModel.FirstEformId != null && workflowSettingsModel.FirstEformId != 0)
                {
                    await _options.UpdateDb(settings =>
                        {
                            settings.FirstEformId = (int)workflowSettingsModel.FirstEformId;
                        },
                        _dbContext,
                        _userService.UserId);
                }

                if (workflowSettingsModel.SecondEformId != null && workflowSettingsModel.SecondEformId != 0)
                {
                    await _options.UpdateDb(settings =>
                        {
                            settings.SecondEformId = (int)workflowSettingsModel.SecondEformId;
                        },
                        _dbContext,
                        _userService.UserId);
                }

                return new OperationResult(
                    true,
                    _workflowLocalizationService.GetString("SettingsHaveBeenUpdatedSuccessfully"));

            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _workflowLocalizationService.GetString("ErrorWhileUpdatingSettings"));
            }
        }

        public async Task<OperationDataResult<Template_Dto>> GetTemplate()
        {
            var core = await _coreHelper.GetCore();

            var language = await _userService.GetCurrentUserLanguage();
            var firstTemplate = await core.TemplateItemRead(_options.Value.FirstEformId, language);
            var secondTemplate = await core.TemplateItemRead(_options.Value.SecondEformId, language);
            return new OperationDataResult<Template_Dto>(true, firstTemplate);
        }
    }
}
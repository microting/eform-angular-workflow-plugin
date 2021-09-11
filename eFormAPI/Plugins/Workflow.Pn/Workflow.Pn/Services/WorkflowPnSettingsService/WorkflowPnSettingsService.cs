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

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Constants;
using Microting.eFormApi.BasePn.Infrastructure.Consts;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using Rebus.Bus;
using Workflow.Pn.Infrastructure.Models;
using Workflow.Pn.Services.RebusService;

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
        private readonly IRebusService _rebusService;
        private readonly IBus _bus;


        public WorkflowPnSettingsService(
            IEFormCoreService coreHelper,
            ILogger<WorkflowPnSettingsService> logger,
            IWorkflowLocalizationService workflowLocalizationService,
            WorkflowPnDbContext dbContext,
            IPluginDbOptions<WorkflowBaseSettings> options,
            IUserService userService,
            IRebusService rebusService)
        {
            _coreHelper = coreHelper;
            _logger = logger;
            _dbContext = dbContext;
            _options = options;
            _userService = userService;
            _workflowLocalizationService = workflowLocalizationService;
            _rebusService = rebusService;
            _bus = _rebusService.GetBus();
        }

        public async Task<OperationDataResult<WorkflowSettingsModel>> GetAllSettingsAsync()
        {
            try
            {
                var assignedSitesIds = await _dbContext.AssignedSites.Where(y => y.WorkflowState != Constants.WorkflowStates.Removed).Select(x => x.SiteMicrotingUid).ToListAsync();
                var workOrdersSettings = new WorkflowSettingsModel()
                {
                    AssignedSites = new List<SiteNameModel>()
                };

                if (assignedSitesIds.Count > 0)
                {
                    var allSites = await _coreHelper.GetCore().Result.SiteReadAll(false);

                    foreach (var id in assignedSitesIds)
                    {
                        var siteNameModel = allSites.Where(x => x.SiteId == id).Select(x => new SiteNameModel()
                        {
                            SiteName = x.SiteName,
                            SiteUId = x.SiteId
                        }).FirstOrDefault();
                        workOrdersSettings.AssignedSites.Add(siteNameModel);
                    }
                }

                var option = _options.Value;

                if (option.FolderId > 0)
                {
                    workOrdersSettings.FolderId = option.FolderId;
                }
                else
                {
                    workOrdersSettings.FolderId = null;
                }
                if (option.FolderTasksId > 0)
                {
                    workOrdersSettings.FolderTasksId = option.FolderTasksId;
                }
                else
                {
                    workOrdersSettings.FolderTasksId = null;
                }

                return new OperationDataResult<WorkflowSettingsModel>(true, workOrdersSettings);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationDataResult<WorkflowSettingsModel>(false,
                    _workflowLocalizationService.GetString("ErrorWhileObtainingWorkOrdersSettings"));
            }
        }

        public async Task<OperationResult> AddSiteToSettingsAsync(int siteId)
        {
            var option = _options.Value;
            var newTaskId = option.FirstEformId;
            var folderId = option.FolderId;
            var theCore = await _coreHelper.GetCore();
            await using var sdkDbContext = theCore.DbContextHelper.GetDbContext();
            var folder = await sdkDbContext.Folders.SingleOrDefaultAsync(x => x.Id == folderId);
            if (folder == null)
            {
                return new OperationResult(false, _workflowLocalizationService.GetString("FolderNotExist"));
            }
            var site = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.MicrotingUid == siteId);
            if (site == null)
            {
                return new OperationResult(false, _workflowLocalizationService.GetString("SiteNotFind"));
            }
            var language = await sdkDbContext.Languages.SingleAsync(x => x.Id == site.LanguageId);
            var mainElement = await theCore.ReadeForm(newTaskId, language);
            switch (language.Name)
            {
                case LanguageNames.Danish:
                {
                    mainElement.Label = "Ny hændelse";
                    break;
                }
                case LanguageNames.English:
                {
                    mainElement.Label = "Near incidet";
                    break;
                }
                case LanguageNames.German:
                {
                    mainElement.Label = "Neuer Vorfall";
                    break;
                }
            }

            mainElement.CheckListFolderName = folder.MicrotingUid.ToString();
            mainElement.EndDate = DateTime.UtcNow.AddYears(10);
            mainElement.Repeated = 0;
            mainElement.PushMessageTitle = mainElement.Label;
            mainElement.PushMessageBody = "";
            var ele = mainElement.ElementList.First();
            ele.Label = mainElement.Label;

            //await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var caseId = await theCore.CaseCreate(mainElement, "", siteId, folderId);
                var assignedSite = new AssignedSite() { SiteMicrotingUid = siteId, CaseMicrotingUid = (int)caseId};

                await assignedSite.Create(_dbContext);

                mainElement = await theCore.ReadeForm(option.InstructionseFormId, language);

                ele = mainElement.ElementList.First();
                mainElement.Label = ele.Label;
                mainElement.CheckListFolderName = folder.MicrotingUid.ToString();
                mainElement.DisplayOrder = int.MinValue;

                await theCore.CaseCreate(mainElement, "", siteId, option.FolderTasksId);
                //await transaction.CommitAsync();
                //await _bus.SendLocal(new SiteAdded(siteId));
                return new OperationResult(true, _workflowLocalizationService.GetString("SiteAddedSuccessfully"));
            }
            catch (Exception e)
            {
                //await transaction.RollbackAsync();
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _workflowLocalizationService.GetString("ErrorWhileAddingSiteToSettings"));
            }
        }

        public async Task<OperationResult> UpdateFolder(int folderId)
        {
            try
            {
                if (folderId > 0)
                {
                    await _options.UpdateDb(settings =>
                        {
                            settings.FolderId = folderId;
                        },
                        _dbContext,
                        _userService.UserId);

                    return new OperationResult(
                        true,
                        _workflowLocalizationService.GetString("FolderUpdatedSuccessfully"));
                }

                throw new ArgumentException($"{nameof(folderId)} is 0");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _workflowLocalizationService.GetString("ErrorWhileUpdatingFolder"));
            }
        }

        public async Task<OperationResult> UpdateTaskFolder(int folderId)
        {
            try
            {
                if (folderId > 0)
                {
                    await _options.UpdateDb(settings =>
                        {
                            settings.FolderTasksId = folderId;
                        },
                        _dbContext,
                        _userService.UserId);

                    return new OperationResult(
                        true,
                        _workflowLocalizationService.GetString("FolderUpdatedSuccessfully"));
                }

                throw new ArgumentException($"{nameof(folderId)} is 0");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _workflowLocalizationService.GetString("ErrorWhileUpdatingFolder"));
            }
        }

        public async Task<OperationResult> RemoveSiteFromSettingsAsync(int siteId)
        {
            try
            {
                var option = _options.Value;
                var assignedSite = await _dbContext.AssignedSites
                    .Where(x => x.SiteMicrotingUid == siteId)
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                    .FirstOrDefaultAsync();
                var theCore = await _coreHelper.GetCore();
                await theCore.CaseDelete((int)assignedSite.CaseMicrotingUid);
                await assignedSite.Delete(_dbContext);

                var dbcontext = theCore.DbContextHelper.GetDbContext();
                var instructionsId = await dbcontext.CheckListSites.SingleOrDefaultAsync(x =>
                    x.CheckListId == option.InstructionseFormId && x.SiteId == siteId);
                if (instructionsId != null)
                {
                    await theCore.CaseDelete(instructionsId.MicrotingUid);
                }

                return new OperationResult(true,
                    _workflowLocalizationService.GetString("SiteDeletedSuccessfully"));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _workflowLocalizationService.GetString("ErrorWhileDeletingSiteFromSettings"));
            }
        }
    }
}
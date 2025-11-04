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
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using Sentry;
using Workflow.Pn.Infrastructure.Models;

namespace Workflow.Pn.Services.WorkflowPnSettingsService;

using Infrastructure.Models.Settings;
using Microsoft.Extensions.Logging;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using System;
using System.Threading.Tasks;
using Microting.eFormWorkflowBase.Infrastructure.Data;
using WorkflowLocalizationService;

public class WorkflowPnSettingsService(
    IEFormCoreService coreHelper,
    ILogger<WorkflowPnSettingsService> logger,
    IWorkflowLocalizationService workflowLocalizationService,
    WorkflowPnDbContext dbContext,
    IPluginDbOptions<WorkflowBaseSettings> options,
    IUserService userService)
    : IWorkflowPnSettingsService
{
    public async Task<OperationDataResult<WorkflowSettingsModel>> GetAllSettingsAsync()
    {
        try
        {
            var core = await coreHelper.GetCore();
            var microuserDbContext = core.DbContextHelper.GetDbContext();
            var assignedSitesIds = await dbContext.AssignedSites
                .Where(y => y.WorkflowState != Constants.WorkflowStates.Removed).Select(x => x.SiteMicrotingUid)
                .ToListAsync();
            var workOrdersSettings = new WorkflowSettingsModel
            {
                AssignedSites = new List<SiteNameModel>()
            };

            if (assignedSitesIds.Count > 0)
            {
                var allSites = await microuserDbContext.Sites
                    .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed).ToListAsync();

                foreach (var id in assignedSitesIds)
                {
                    var siteNameModel = allSites.Where(x => x.MicrotingUid == id).Select(x => new SiteNameModel
                    {
                        SiteName = x.Name,
                        SiteUId = x.MicrotingUid!.Value
                    }).FirstOrDefault();
                    if (siteNameModel != null)
                    {
                        workOrdersSettings.AssignedSites.Add(siteNameModel);
                    }
                    else
                    {
                        logger.LogError($"Could not find an active site for {id}");
                    }
                }
            }

            var option = options.Value;

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
            SentrySdk.CaptureException(e);
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
            return new OperationDataResult<WorkflowSettingsModel>(false,
                workflowLocalizationService.GetString("ErrorWhileObtainingWorkOrdersSettings"));
        }
    }

    public async Task<OperationResult> AddSiteToSettingsAsync(int siteId)
    {
        var option = options.Value;
        var newTaskId = option.FirstEformId;
        var folderId = option.FolderId;
        var theCore = await coreHelper.GetCore();
        await using var sdkDbContext = theCore.DbContextHelper.GetDbContext();
        var folder = await sdkDbContext.Folders.SingleOrDefaultAsync(x => x.Id == folderId);
        if (folder == null)
        {
            return new OperationResult(false, workflowLocalizationService.GetString("FolderNotExist"));
        }

        var site = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.MicrotingUid == siteId);
        if (site == null)
        {
            return new OperationResult(false, workflowLocalizationService.GetString("SiteNotFind"));
        }

        var language = await sdkDbContext.Languages.SingleAsync(x => x.Id == site.LanguageId);
        var mainElement = await theCore.ReadeForm(newTaskId, language);
        switch (language.LanguageCode)
        {
            case "da":
            {
                mainElement.Label = "Ny hændelse";
                break;
            }
            case "en-US":
            {
                mainElement.Label = "New incident";
                break;
            }
            case "de-DE":
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
            var assignedSite = new AssignedSite() { SiteMicrotingUid = siteId, CaseMicrotingUid = (int)caseId };

            await assignedSite.Create(dbContext);

            mainElement = await theCore.ReadeForm(option.InstructionseFormId, language);

            ele = mainElement.ElementList.First();
            mainElement.Label = ele.Label;
            folderId = option.FolderTasksId;
            folder = await sdkDbContext.Folders.SingleOrDefaultAsync(x => x.Id == folderId);
            mainElement.CheckListFolderName = folder.MicrotingUid.ToString();
            mainElement.EndDate = DateTime.UtcNow.AddYears(10);
            mainElement.DisplayOrder = int.MaxValue;

            await theCore.CaseCreate(mainElement, "", siteId, option.FolderTasksId);
            return new OperationResult(true, workflowLocalizationService.GetString("SiteAddedSuccessfully"));
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
            return new OperationResult(false,
                workflowLocalizationService.GetString("ErrorWhileAddingSiteToSettings"));
        }
    }

    public async Task<OperationResult> UpdateFolder(int folderId)
    {
        try
        {
            if (folderId > 0)
            {
                await options.UpdateDb(settings => { settings.FolderId = folderId; },
                    dbContext,
                    userService.UserId);

                return new OperationResult(
                    true,
                    workflowLocalizationService.GetString("FolderUpdatedSuccessfully"));
            }

            throw new ArgumentException($"{nameof(folderId)} is 0");
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
            return new OperationResult(false,
                workflowLocalizationService.GetString("ErrorWhileUpdatingFolder"));
        }
    }

    public async Task<OperationResult> UpdateTaskFolder(int folderId)
    {
        try
        {
            if (folderId > 0)
            {
                await options.UpdateDb(settings => { settings.FolderTasksId = folderId; },
                    dbContext,
                    userService.UserId);

                return new OperationResult(
                    true,
                    workflowLocalizationService.GetString("FolderUpdatedSuccessfully"));
            }

            throw new ArgumentException($"{nameof(folderId)} is 0");
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
            return new OperationResult(false,
                workflowLocalizationService.GetString("ErrorWhileUpdatingFolder"));
        }
    }

    public async Task<OperationResult> RemoveSiteFromSettingsAsync(int siteId)
    {
        try
        {
            var option = options.Value;
            var assignedSite = await dbContext.AssignedSites
                .Where(x => x.SiteMicrotingUid == siteId)
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .FirstOrDefaultAsync();
            var theCore = await coreHelper.GetCore();
            await theCore.CaseDelete((int)assignedSite.CaseMicrotingUid);
            await assignedSite.Delete(dbContext);

            var dbcontext = theCore.DbContextHelper.GetDbContext();
            var checkListSites = await dbcontext.CheckListSites.Where(x =>
                x.CheckListId == option.InstructionseFormId && x.SiteId == siteId &&
                x.WorkflowState != Constants.WorkflowStates.Removed).ToListAsync();
            if (checkListSites != null)
            {
                foreach (var checkListSite in checkListSites)
                {
                    await theCore.CaseDelete(checkListSite.MicrotingUid);
                }
            }

            return new OperationResult(true,
                workflowLocalizationService.GetString("SiteDeletedSuccessfully"));
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            logger.LogError(e.Message);
            logger.LogTrace(e.StackTrace);
            return new OperationResult(false,
                workflowLocalizationService.GetString("ErrorWhileDeletingSiteFromSettings"));
        }
    }
}
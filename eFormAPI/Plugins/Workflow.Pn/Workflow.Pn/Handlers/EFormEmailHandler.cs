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

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.EformAngularFrontendBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Helpers;
using Microting.eFormWorkflowBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using Microting.eFormWorkflowBase.Messages;
using Rebus.Handlers;
using Workflow.Pn.Infrastructure.Helpers;

namespace Workflow.Pn.Handlers;

public class EFormEmailHandler : IHandleMessages<QueueEformEmail>
{
    private readonly eFormCore.Core _sdkCore;
    private readonly WorkflowPnDbContext _dbContext;
    private readonly BaseDbContext _baseDbContext;
    private readonly EmailHelper _emailHelper;

    public EFormEmailHandler(eFormCore.Core sdkCore, DbContextHelper dbContextHelper, BaseDbContext baseDbContext, EmailHelper emailHelper)
    {
        _dbContext = dbContextHelper.GetDbContext();
        _sdkCore = sdkCore;
        _baseDbContext = baseDbContext;
        _emailHelper = emailHelper;
    }

    public async Task Handle(QueueEformEmail message)
    {
        WorkflowCase workflowCase = await _dbContext.WorkflowCases.SingleOrDefaultAsync(x => x.Id == message.CaseId);
        await using MicrotingDbContext sdkDbContext = _sdkCore.DbContextHelper.GetDbContext();
        Microting.eForm.Infrastructure.Data.Entities.Case _case = await
            sdkDbContext.Cases.SingleOrDefaultAsync(x => x.MicrotingCheckUid == workflowCase.CheckMicrotingUid);
        Site createdBySite = await sdkDbContext.Sites.SingleOrDefaultAsync(x => x.Id == _case.SiteId);

        await _emailHelper.GenerateReportAndSendEmail(createdBySite.LanguageId, createdBySite.Name, workflowCase);

        if (!string.IsNullOrEmpty(workflowCase.SolvedBy))
        {
            Site site = await sdkDbContext.Sites.SingleOrDefaultAsync(x =>
                x.Name == workflowCase.SolvedBy);

            if (workflowCase.SolvedBy != createdBySite.Name)
            {
                await _emailHelper.GenerateReportAndSendEmail(site.LanguageId, site.Name, workflowCase);
            }
        }
    }
}
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

using System.Text;
using Workflow.Pn.Services.WorkflowLocalizationService;

namespace Workflow.Pn.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microting.eFormApi.BasePn.Infrastructure.Models.API;
    using System.Threading.Tasks;
    using Infrastructure.Models;
    using Infrastructure.Models.Cases;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Common;
    using Services.WorkflowCasesService;

    [Route("api/workflow-pn/cases")]
    public class WorkflowCasesController : Controller
    {

        private readonly IWorkflowCasesService _workflowPnSettingsService;
        private readonly IWorkflowLocalizationService _workflowLocalizationService;

        public WorkflowCasesController(IWorkflowCasesService workflowPnSettingsService, IWorkflowLocalizationService workflowLocalizationService)
        {
            _workflowPnSettingsService = workflowPnSettingsService;
            _workflowLocalizationService = workflowLocalizationService;
        }

        [HttpPut]
        [Authorize]
        public async Task<OperationResult> UpdateWorkflowCase([FromBody] WorkflowCasesUpdateModel model)
        {
            return await _workflowPnSettingsService.UpdateWorkflowCase(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperationDataResult<Paged<WorkflowCasesModel>>> Index([FromBody] WorkflowCasesResponse model)
        {
            return await _workflowPnSettingsService.Index(model);
        }


        [HttpGet]
        [Authorize]
        public async Task<OperationDataResult<WorkflowCasesUpdateModel>> Read(int id)
        {
            return await _workflowPnSettingsService.Read(id);
        }

        [HttpGet]
        [Authorize]
        [Route("places")]
        public async Task<OperationDataResult<List<WorkflowPlacesModel>>> GetPlaces()
        {
            return await _workflowPnSettingsService.GetPlaces();
        }

        [HttpDelete]
        [Authorize]
        public async Task<OperationResult> Delete(int id)
        {
            return await _workflowPnSettingsService.Delete(id);
        }

        [HttpGet]
        [Authorize]
        [Route("download-case-pdf")]
        public async Task DownloadEFormPdf(int id, string fileType)
        {
            var result =  await _workflowPnSettingsService.DownloadEFormPdf(id, fileType);
            const int bufferSize = 4086;
            var buffer = new byte[bufferSize];
            Response.OnStarting(async () =>
            {
                if (!result.Success)
                {
                    Response.ContentLength = result.Message.Length;
                    Response.ContentType = "text/plain";
                    Response.StatusCode = 400;
                    var bytes = Encoding.UTF8.GetBytes(result.Message);
                    await Response.Body.WriteAsync(bytes, 0, result.Message.Length);
                    await Response.Body.FlushAsync();
                }
                else
                {
                    await using var wordStream = result.Model;
                    int bytesRead;
                    Response.ContentLength = wordStream.Length;
                    Response.ContentType = fileType == "docx"
                        ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        : "application/pdf";

                    while ((bytesRead = await wordStream.ReadAsync(buffer, 0, buffer.Length)) > 0 &&
                           !HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        await Response.Body.WriteAsync(buffer, 0, bytesRead);
                        await Response.Body.FlushAsync();
                    }
                }
            });
        }

        [HttpGet]
        [Authorize]
        [Route("download-cases-as-xlsx")]
        public async Task<IActionResult> DownloadCasesAsXlsx()
        {
            var result =  await _workflowPnSettingsService.DownloadCasesAsXlsx();
            return File(result, "application/pdf", _workflowLocalizationService.GetString("Incidents") + ".xlsx");
        }
    }
}
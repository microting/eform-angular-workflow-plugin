using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.EformAngularFrontendBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Helpers;
using Microting.eFormWorkflowBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Workflow.Pn.Infrastructure.Helpers;

public class EmailHelper
{
    private readonly eFormCore.Core _sdkCore;
    private readonly WorkflowPnDbContext _dbContext;
    private readonly BaseDbContext _baseDbContext;
    private readonly WorkflowReportHelper _workflowReportHelper;

    public EmailHelper(eFormCore.Core sdkCore, DbContextHelper dbContextHelper, BaseDbContext baseDbContext, WorkflowReportHelper workflowReportHelper)
    {
        _dbContext = dbContextHelper.GetDbContext();
        _sdkCore = sdkCore;
        _baseDbContext = baseDbContext;
        _workflowReportHelper = workflowReportHelper;
    }

    private async Task SendFileAsync(
        string fromEmail,
        string fromName,
        string subject,
        string to,
        string fileName,
        string text = null, string html = null)
    {
        try
        {
            var sendGridKey =
                _baseDbContext.ConfigurationValues.Single(x => x.Id == "EmailSettings:SendGridKey");
            var client = new SendGridClient(sendGridKey.Value);
            var fromEmailAddress = new EmailAddress(fromEmail.Replace(" ", ""), fromName);
            var toEmail = new EmailAddress(to.Replace(" ", ""));
            var msg = MailHelper.CreateSingleEmail(fromEmailAddress, toEmail, subject, text, html);
            var bytes = await File.ReadAllBytesAsync(fileName);
            var file = Convert.ToBase64String(bytes);
            msg.AddAttachment(Path.GetFileName(fileName), file);
            var response = await client.SendEmailAsync(msg);
            if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
            {
                throw new Exception($"Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to send email message", ex);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    public async Task GenerateReportAndSendEmail(int languageId, string userName, WorkflowCase workflowCase)
    {
        var emailRecipient = await _baseDbContext.EmailRecipients.SingleOrDefaultAsync(x => x.Name == userName
            .Replace("Mobil", "")
            .Replace("Tablet", ""));
        var filePath = await _workflowReportHelper.GenerateReportAnd(languageId, workflowCase, "pdf");
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name;

        var stream = assembly.GetManifestResourceStream($"{assemblyName}.Resources.Email.html");
        string html;
        if (stream == null)
        {
            throw new InvalidOperationException("Resource not found");
        }
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            html = await reader.ReadToEndAsync();
        }
        html = html
            .Replace(
                "<a href=\"{{link}}\">Link til sag</a>", "")
            .Replace("{{CreatedBy}}", workflowCase.CreatedBySiteName)
            .Replace("{{CreatedAt}}", workflowCase.DateOfIncident.ToString("dd-MM-yyyy"))
            .Replace("{{Type}}", workflowCase.IncidentType)
            .Replace("{{Location}}", workflowCase.IncidentPlace)
            .Replace("{{Description}}", workflowCase.Description.Replace("&", "&amp;"))
            .Replace("<p>Ansvarlig: {{SolvedBy}}</p>", "")
            .Replace("<p>Handlingsplan: {{ActionPlan}}</p>", "");

        await SendFileAsync(
            "no-reply@microting.com",
            userName,
            $"{workflowCase.IncidentType};  {workflowCase.IncidentPlace}; {workflowCase.CreatedAt:dd-MM-yyyy}",
            emailRecipient?.Email,
            filePath,
            null,
            html);
    }
}
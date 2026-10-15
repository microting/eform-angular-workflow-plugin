using System;
using System.Linq;
using System.Threading.Tasks;
using eFormCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eForm.Infrastructure.Helpers;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Database.Entities;
using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
using Microting.eFormWorkflowBase.Infrastructure.Data;
using Microting.eFormWorkflowBase.Infrastructure.Data.Entities;
using NSubstitute;
using NUnit.Framework;
using Testcontainers.MariaDb;
using Workflow.Pn.Infrastructure.Models.Settings;
using Workflow.Pn.Services.WorkflowCasesService;
using Workflow.Pn.Services.WorkflowLocalizationService;

#nullable enable
namespace Workflow.Pn.Test;

/// <summary>
/// microting/eform-angular-frontend#8141: on a MariaDB Galera cluster
/// (auto_increment_increment=3) the SDK lookup tables get non-contiguous ids
/// (Picture=15, SingleSelect=24, Dansk=3, ...). The workflow plugin must
/// resolve field types by FieldType.Type, never by a hardcoded numeric id.
/// </summary>
[TestFixture]
public class ShiftedLookupIdsTests
{
    // Mimic a Galera node: every AUTO_INCREMENT step is 3, starting at 3,
    // so the SDK field types seed as Text=3 .. Picture=15 .. SingleSelect=24.
    private readonly MariaDbContainer _container = new MariaDbBuilder("mariadb:11.2")
        .WithDatabase("myDb").WithUsername("bla").WithPassword("secretpassword")
        .WithCommand("--auto-increment-increment=3", "--auto-increment-offset=3")
        .Build();

    private string _sdkConnection = null!;
    private MicrotingDbContext _sdkDbContext = null!;
    private WorkflowPnDbContext _workflowDbContext = null!;
    private Language _danish = null!;

    // SDK seeding order (SqlController.SettingCheckAll)
    private static readonly string[] SdkFieldTypeOrder =
    {
        Constants.FieldTypes.Text, Constants.FieldTypes.Number, Constants.FieldTypes.None,
        Constants.FieldTypes.CheckBox, Constants.FieldTypes.Picture, Constants.FieldTypes.Audio,
        Constants.FieldTypes.Movie, Constants.FieldTypes.SingleSelect, Constants.FieldTypes.Comment,
        Constants.FieldTypes.MultiSelect, Constants.FieldTypes.Date, Constants.FieldTypes.Signature,
        Constants.FieldTypes.Timer, Constants.FieldTypes.EntitySearch, Constants.FieldTypes.EntitySelect,
        Constants.FieldTypes.ShowPdf, Constants.FieldTypes.FieldGroup, Constants.FieldTypes.SaveButton,
        Constants.FieldTypes.NumberStepper
    };

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _container.StartAsync();
        var conn = _container.GetConnectionString();
        _sdkConnection = conn.Replace("myDb", "420_SDK").Replace("bla", "root");
        var pluginConnection = conn.Replace("myDb", "420_eform-angular-workflow-plugin").Replace("bla", "root");

        _sdkDbContext = new DbContextHelper(_sdkConnection).GetDbContext();
        await _sdkDbContext.Database.MigrateAsync();

        foreach (var type in SdkFieldTypeOrder)
        {
            if (!await _sdkDbContext.FieldTypes.AnyAsync(x => x.Type == type))
            {
                await new FieldType { Type = type, Description = type }.Create(_sdkDbContext);
            }
        }

        _danish = await _sdkDbContext.Languages.FirstOrDefaultAsync(x => x.LanguageCode == "da");
        if (_danish == null)
        {
            _danish = new Language { Name = "Dansk", LanguageCode = "da" };
            await _danish.Create(_sdkDbContext);
        }

        var optionsBuilder = new DbContextOptionsBuilder<WorkflowPnDbContext>();
        optionsBuilder.UseMySql(pluginConnection,
            new MariaDbServerVersion(ServerVersion.AutoDetect(pluginConnection)));
        _workflowDbContext = new WorkflowPnDbContext(optionsBuilder.Options);
        await _workflowDbContext.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
        if (_workflowDbContext != null) await _workflowDbContext.DisposeAsync();
        if (_sdkDbContext != null) await _sdkDbContext.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _workflowDbContext.PluginConfigurationValues.RemoveRange(_workflowDbContext.PluginConfigurationValues);
        await _workflowDbContext.SaveChangesAsync();
    }

    [Test]
    public async Task FieldTypeIds_AreShiftedLikeAGaleraInstall()
    {
        // Guard: the fixture really does produce shifted ids, otherwise the
        // tests below would pass for the wrong reason.
        Assert.That(await FieldTypeId(Constants.FieldTypes.Picture), Is.EqualTo(15));
        Assert.That(await FieldTypeId(Constants.FieldTypes.SingleSelect), Is.EqualTo(24));
    }

    [Test]
    public async Task Read_ResolvesPictureFieldByType_WhenPictureIdIsShifted()
    {
        var secondEform = await CreateCheckList("second eform", parentId: null);
        var child = await CreateCheckList("second eform child", parentId: secondEform.Id);
        await CreateField(child.Id, Constants.FieldTypes.Text, "Text");
        await CreateField(child.Id, Constants.FieldTypes.Picture, "Pictures of task done");

        await AddConfig($"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.SecondEformId)}", secondEform.Id.ToString());
        await AddConfig($"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentPlaceListId)}", "1");
        await AddConfig($"WorkflowBaseSettings:{nameof(WorkflowBaseSettings.IncidentTypeListId)}", "2");

        var workflowCase = new WorkflowCase
        {
            Description = "desc",
            DateOfIncident = DateTime.UtcNow,
            IncidentPlace = "place",
            IncidentType = "type",
        };
        await workflowCase.Create(_workflowDbContext);

        var result = await CreateService(new WorkflowBaseSettings()).Read(workflowCase.Id);

        Assert.That(result.Success, Is.True, result.Message);
        Assert.That(result.Model.Id, Is.EqualTo(workflowCase.Id));
    }

    [Test]
    public async Task GetPlaces_ResolvesSingleSelectFieldsByType_WhenSingleSelectIdIsShifted()
    {
        var firstEform = await CreateCheckList("first eform", parentId: null);
        await CreateField(firstEform.Id, Constants.FieldTypes.Text, "Text");
        await CreateField(firstEform.Id, Constants.FieldTypes.SingleSelect, "Incident type");
        var placesField = await CreateField(firstEform.Id, Constants.FieldTypes.SingleSelect, "Incident place");
        await CreateOption(placesField.Id, "Kontoret");
        await CreateOption(placesField.Id, "Lageret");

        var result = await CreateService(new WorkflowBaseSettings { FirstEformId = firstEform.Id }).GetPlaces();

        Assert.That(result.Success, Is.True, result.Message);
        Assert.That(result.Model.Select(x => x.Name), Is.EquivalentTo(new[] { "Kontoret", "Lageret" }));
    }

    private WorkflowCasesService CreateService(WorkflowBaseSettings settings)
    {
        var core = new Core { DbContextHelper = new DbContextHelper(_sdkConnection) };
        var coreService = Substitute.For<IEFormCoreService>();
        coreService.GetCore().Returns(core);

        var userService = Substitute.For<IUserService>();
        userService.GetCurrentUserLanguage().Returns(_danish);

        var options = Substitute.For<IPluginDbOptions<WorkflowBaseSettings>>();
        options.Value.Returns(settings);

        return new WorkflowCasesService(
            coreService,
            userService,
            _workflowDbContext,
            Substitute.For<IWorkflowLocalizationService>(),
            options,
            NullLogger<WorkflowCasesService>.Instance,
            null!);
    }

    private async Task<int> FieldTypeId(string type) =>
        (await _sdkDbContext.FieldTypes.SingleAsync(x => x.Type == type)).Id;

    private async Task<CheckList> CreateCheckList(string label, int? parentId)
    {
        var checkList = new CheckList { Label = label, ParentId = parentId };
        await checkList.Create(_sdkDbContext);
        return checkList;
    }

    private async Task<Field> CreateField(int checkListId, string type, string label)
    {
        var field = new Field
        {
            CheckListId = checkListId,
            FieldTypeId = await FieldTypeId(type),
            Label = label
        };
        await field.Create(_sdkDbContext);
        return field;
    }

    private async Task CreateOption(int fieldId, string text)
    {
        var option = new FieldOption { FieldId = fieldId, Key = text };
        await option.Create(_sdkDbContext);
        await new FieldOptionTranslation
        {
            FieldOptionId = option.Id,
            LanguageId = _danish.Id,
            Text = text
        }.Create(_sdkDbContext);
    }

    private async Task AddConfig(string name, string value)
    {
        _workflowDbContext.PluginConfigurationValues.Add(new PluginConfigurationValue { Name = name, Value = value });
        await _workflowDbContext.SaveChangesAsync();
    }
}

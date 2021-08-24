using eFormCore;
using Microting.eForm.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using EntityGroup = Microting.eForm.Infrastructure.Models.EntityGroup;

namespace Workflow.Pn.Helpers
{

    using Microting.eForm.Dto;

    public class SeedHelper
    {
        public static async Task<int> CreateAccidentTypesList(Core core)
        {
            EntityGroupList model = await core.Advanced_EntityGroupAll(
                "id",
                "eform-angular-workflow-plugin-editable-AccidentType",
                0, 1, Constants.FieldTypes.EntitySelect,
                false,
                Constants.WorkflowStates.NotRemoved);

            EntityGroup group;

            if (!model.EntityGroups.Any())
            {
                group = await core.EntityGroupCreate(Constants.FieldTypes.EntitySelect,
                    "eform-angular-workflow-plugin-editable-AccidentType", "Hændelsestyper");
            }
            else
            {
                group = model.EntityGroups.First();
            }

            return int.Parse(group.MicrotingUUID);
        }

        public static async Task<int> CreateAccidentLocationList(Core core)
        {
            EntityGroupList model = await core.Advanced_EntityGroupAll(
                "id",
                "eform-angular-workflow-plugin-editable-AccidentLocations",
                0, 1, Constants.FieldTypes.EntitySelect,
                false,
                Constants.WorkflowStates.NotRemoved);

            EntityGroup group;

            if (!model.EntityGroups.Any())
            {
                group = await core.EntityGroupCreate(Constants.FieldTypes.EntitySelect,
                    "eform-angular-workflow-plugin-editable-AccidentLocations", "Hændelsestyper");
            }
            else
            {
                group = model.EntityGroups.First();
            }

            return int.Parse(group.MicrotingUUID);
        }

        public static async Task<int> CreateNewTaskEform(Core core)
        {

            const string timeZone = "Europe/Copenhagen";
            TimeZoneInfo timeZoneInfo;

            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            }
            int accidentTypesList = await CreateAccidentTypesList(core);
            int accidentLocationList = await CreateAccidentLocationList(core);
            Language language = await core.DbContextHelper.GetDbContext().Languages.FirstAsync();

            List<Template_Dto> templatesDto = await core.TemplateItemReadAll(false,
                "",
                "eform-angular-workflow-plugin-newtask",
                false,
                "",
                new List<int>(),
                timeZoneInfo,
                language
                );

            if (templatesDto.Count > 0)
            {
                return templatesDto.First().Id;
            }

            MainElement newTaskForm = new MainElement
            {
                Id = 5769,
                Repeated = 0,
                Label = "eform-angular-workflow-plugin-newtask|eform-angular-workflow-plugin-newtask|eform-angular-workflow-plugin-newtask",
                StartDate = new DateTime(2020, 09, 14),
                EndDate = new DateTime(2030, 09, 14),
                Language = "da",
                MultiApproval = false,
                FastNavigation = false,
                DisplayOrder = 0,
                EnableQuickSync = true
            };

            List<DataItem> dataItems = new List<DataItem>
            {
                new Date(
                    371265,
                    true,
                    false,
                    "Dato for hændelse|Datum des Vorfalls|",
                    "",
                    Constants.FieldColors.Default,
                    0,
                    false,
                    new DateTime(),
                    new DateTime(),
                    ""
                ),
                new EntitySelect(
                    371261,
                    false,
                    false,
                    "Type|Incident|Vorfall",
                    "",
                    Constants.FieldColors.Default,
                    1,
                    false,
                    0,
                    accidentTypesList),
                new EntitySelect(
                    371262,
                    false,
                    false,
                    "Sted|Location of incident|Ort des Vorfalls",
                    "",
                    Constants.FieldColors.Default,
                    2,
                    false,
                    0,
                    accidentLocationList),
                new Picture(
                    371263,
                    false,
                    false,
                    "Foto|Picture of incident|Bild des Vorfalls",
                    "",
                    Constants.FieldColors.Default,
                    3,
                    false,
                    0,
                    false
                ),
                new Comment(
                    371264,
                    true,
                    false,
                    "Beskrivelse|Description of incident|Beschreibung des Vorfalls",
                    "",
                    Constants.FieldColors.Default,
                    4,
                    false,
                    "",
                    0,
                    false
                ),
                new SaveButton(
                    371266,
                    false,
                    false,
                    "Gem og send|Save and send|Speichern und senden",
                    "",
                    Constants.FieldColors.Green,
                    5,
                    false,
                    "Gem og send|Save and send|Speichern und senden"
                )
            };


            DataElement dataElement = new DataElement(
                142108,
                "Ny opgave|New task|Neue Aufgabe",
                0,
                "", // ?
                false,
                false,
                false,
                false,
                "",
                false,
                new List<DataItemGroup>(),
                dataItems);

            newTaskForm.ElementList.Add(dataElement);

            newTaskForm = await core.TemplateUploadData(newTaskForm);
            return await core.TemplateCreate(newTaskForm);
        }

        public static async Task<int> CreateTaskListEform(Core core)
        {
            string timeZone = "Europe/Copenhagen";
            TimeZoneInfo timeZoneInfo;

            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            }

            Language language = await core.DbContextHelper.GetDbContext().Languages.FirstAsync();
            List<Template_Dto> templatesDto = await core.TemplateItemReadAll(false,
                    "",
                    "eform-angular-workflow-plugin-tasklist",
                    false,
                    "",
                    new List<int>(),
                    timeZoneInfo,
                    language
                    );

            if (templatesDto.Count > 0)
            {
                return templatesDto.First().Id;
            }

            MainElement taskListForm = new MainElement
            {
                Id = 7680,
                Repeated = 0,
                Label = "eform-angular-workflow-plugin-tasklist|eform-angular-workflow-plugin-tasklist|eform-angular-workflow-plugin-tasklist",
                StartDate = new DateTime(2020, 09, 14),
                EndDate = new DateTime(2030, 09, 14),
                Language = "da",
                MultiApproval = false,
                FastNavigation = false,
                DisplayOrder = 0,
                EnableQuickSync = true
            };

            List<DataItem> dataItems = new List<DataItem>
            {
                new None(
                    371267,
                    false,
                    false,
                    "INFO|INFO|DIE INFO",
                    "",
                    Constants.FieldColors.Yellow,
                    0,
                    false
                ),
                new ShowPdf(
                    371268,
                    false,
                    false,
                    "Tryk på PDF for at se billeder af hændelse|Click on PDF to see pictures of the incident|Klicken Sie auf das PDF, um Bilder des Vorfalls zu sehen",
                    "",
                    Constants.FieldColors.Default,
                    1,
                    false,
                    "https://eform.microting.com/app_files/uploads/20200914114927_14937_9fae9a0b11bda418201523437984027c.pdf"
                ),
                new Comment(
                    371271,
                    false,
                    false,
                    "Beskrivelse|Description of incident|Beschreibung des Vorfalls",
                    "",
                    Constants.FieldColors.Default,
                    2,
                    false,
                    "",
                    0,
                    false
                    ),
                new Comment(
                    371271,
                    false,
                    false,
                    "Handlingsplan|Action plan|Aktionsplan",
                    "",
                    Constants.FieldColors.Default,
                    3,
                    false,
                    "",
                    0,
                    false
                ),
                new Picture(
                    371270,
                    false,
                    false,
                    "Billede af udført opgave|Picture of completed task|Bild der erledigten Aufgabe",
                    "",
                    Constants.FieldColors.Default,
                    4,
                    false,
                    0,
                    false
                ),
                new SaveButton(
                    371266,
                    false,
                    false,
                    "Gem og send|Save and send|Speichern und senden",
                    "",
                    Constants.FieldColors.Green,
                    5,
                    false,
                    "Gem og send|Save and send|Speichern und senden"
                )
            };


            DataElement dataElement = new DataElement(
                142109,
                "Hændelse registreret|Incident registered|Vorfall registriert",
                0,
                "",
                false,
                false,
                false,
                false,
                "",
                false,
                new List<DataItemGroup>(),
                dataItems);

            taskListForm.ElementList.Add(dataElement);

            taskListForm = await core.TemplateUploadData(taskListForm);
            return await core.TemplateCreate(taskListForm);
        }
    }
}
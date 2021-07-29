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


using eFormCore;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
using Workflow.Pn.Helpers;

namespace Workflow.Pn
{
    using Infrastructure.Data.Seed;
    using Infrastructure.Data.Seed.Data;
    using Infrastructure.Models.Settings;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microting.eFormApi.BasePn;
    using Microting.eFormApi.BasePn.Infrastructure.Consts;
    using Microting.eFormApi.BasePn.Infrastructure.Database.Extensions;
    using Microting.eFormApi.BasePn.Infrastructure.Helpers;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Application;
    using Microting.eFormApi.BasePn.Infrastructure.Models.Application.NavigationMenu;
    using Microting.eFormApi.BasePn.Infrastructure.Settings;
    using Services.WorkflowPnSettingsService;
    using Services.RebusService;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microting.eFormWorkflowBase.Infrastructure.Data;
    using Microting.eFormWorkflowBase.Infrastructure.Data.Factories;
    using Services.WorkflowCasesService;
    using Services.WorkflowLocalizationService;

    public class EformWorkflowPlugin : IEformPlugin
    {
        public string Name => "Microting Workflow Plugin";

        public string PluginId => "eform-angular-workflow-plugin";

        public string PluginPath => PluginAssembly().Location;

        public string PluginBaseUrl => "workflow-pn";

        private string _connectionString;

        public Assembly PluginAssembly()
        {
            return typeof(EformWorkflowPlugin).GetTypeInfo().Assembly;
        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            var serviceProvider = appBuilder.ApplicationServices;

            var rabbitMqHost = "localhost";

            if (_connectionString.Contains("frontend"))
            {
                var dbPrefix = Regex.Match(_connectionString, @"atabase=(\d*)_").Groups[1].Value;
                rabbitMqHost = $"frontend-{dbPrefix}-rabbitmq";
            }

            var rebusService = serviceProvider.GetService<IRebusService>();
            rebusService.Start(_connectionString, "admin", "password", rabbitMqHost);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWorkflowLocalizationService, WorkflowLocalizationService>();
            services.AddTransient<IWorkflowPnSettingsService, WorkflowPnSettingsService>();
            services.AddTransient<IWorkflowCasesService, WorkflowCasesService>();
            services.AddSingleton<IRebusService, RebusService>();
            services.AddControllers();
            SeedWorkOrderForms(services);
        }

        public void ConfigureOptionsServices(IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigurePluginDbOptions<WorkflowBaseSettings>(
                configuration.GetSection("WorkflowBaseSettings"));
        }

        public void ConfigureDbContext(IServiceCollection services, string connectionString)
        {
            _connectionString = connectionString;
            services.AddDbContext<WorkflowPnDbContext>(o => o.UseMySql(connectionString, new MariaDbServerVersion(
                new Version(10, 4, 0)), mySqlOptionsAction: builder =>
            {
                builder.EnableRetryOnFailure();
                builder.MigrationsAssembly(PluginAssembly().FullName);
            }));

            var contextFactory = new WorkflowPnContextFactory();
            var context = contextFactory.CreateDbContext(new[] { connectionString });
            context.Database.Migrate();

            // Seed database
            SeedDatabase(connectionString);
        }

        public MenuModel HeaderMenu(IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider
                .GetService<IWorkflowLocalizationService>();

            var result = new MenuModel();
            result.LeftMenu.Add(new MenuItemModel
            {
                Name = localizationService.GetString("Workflow"),
                E2EId = "workflow-pn",
                Link = "",
                Guards = new List<string>(),
                MenuItems = new List<MenuItemModel>
                {
                    new MenuItemModel
                    {
                        Name = localizationService.GetString("Cases"),
                        E2EId = "workflow-pn-cases",
                        Link = "/plugins/workflow-pn/cases",
                        Guards = new List<string>(),
                        Position = 0,
                    },
                },
            });
            return result;
        }

        public List<PluginMenuItemModel> GetNavigationMenu(IServiceProvider serviceProvider)
        {
            var pluginMenu = new List<PluginMenuItemModel>
                {
                    new PluginMenuItemModel
                    {
                        Name = "Dropdown",
                        E2EId = "workflow-pn",
                        Link = "",
                        Type = MenuItemTypeEnum.Dropdown,
                        Position = 0,
                        Translations = new List<PluginMenuTranslationModel>
                        {
                            new PluginMenuTranslationModel
                            {
                                 LocaleName = LocaleNames.English,
                                 Name = "Workflow",
                                 Language = LanguageNames.English,
                            },
                            new PluginMenuTranslationModel
                            {
                                 LocaleName = LocaleNames.German,
                                 Name = "Arbeitsablauf",
                                 Language = LanguageNames.German,
                            },
                            new PluginMenuTranslationModel
                            {
                                 LocaleName = LocaleNames.Danish,
                                 Name = "Arbejdsproces",
                                 Language = LanguageNames.Danish,
                            },
                        },
                        ChildItems = new List<PluginMenuItemModel>
                        {
                            new PluginMenuItemModel
                            {
                                Name = "Cases",
                                E2EId = "workflow-pn-cases",
                                Link = "/plugins/workflow-pn/cases",
                                Type = MenuItemTypeEnum.Link,
                                Position = 0,
                                MenuTemplate = new PluginMenuTemplateModel()
                                {
                                    Name = "Cases",
                                    E2EId = "workflow-pn-cases",
                                    DefaultLink = "/plugins/workflow-pn/cases",
                                    Permissions = new List<PluginMenuTemplatePermissionModel>(),
                                    Translations = new List<PluginMenuTranslationModel>
                                    {
                                        new PluginMenuTranslationModel
                                        {
                                            LocaleName = LocaleNames.English,
                                            Name = "Cases",
                                            Language = LanguageNames.English,
                                        },
                                        new PluginMenuTranslationModel
                                        {
                                            LocaleName = LocaleNames.German,
                                            Name = "Fälle",
                                            Language = LanguageNames.German,
                                        },
                                        new PluginMenuTranslationModel
                                        {
                                            LocaleName = LocaleNames.Danish,
                                            Name = "Tilfælde",
                                            Language = LanguageNames.Danish,
                                        },
                                    }
                                },
                                Translations = new List<PluginMenuTranslationModel>
                                {
                                    new PluginMenuTranslationModel
                                    {
                                        LocaleName = LocaleNames.English,
                                        Name = "Cases",
                                        Language = LanguageNames.English,
                                    },
                                    new PluginMenuTranslationModel
                                    {
                                        LocaleName = LocaleNames.German,
                                        Name = "Fälle",
                                        Language = LanguageNames.German,
                                    },
                                    new PluginMenuTranslationModel
                                    {
                                        LocaleName = LocaleNames.Danish,
                                        Name = "Tilfælde",
                                        Language = LanguageNames.Danish,
                                    },
                                }
                            },
                        }
                    }
                };

            return pluginMenu;
        }

        public void SeedDatabase(string connectionString)
        {
            // Get DbContext
            var contextFactory = new WorkflowPnContextFactory();
            using var context = contextFactory.CreateDbContext(new[] { connectionString });

            // Seed configuration
            WorkflowPluginSeed.SeedData(context);
        }

        public void AddPluginConfig(IConfigurationBuilder builder, string connectionString)
        {
            var seedData = new WorkflowConfigurationSeedData();
            var contextFactory = new WorkflowPnContextFactory();
            builder.AddPluginConfiguration(
                connectionString,
                seedData,
                contextFactory);
        }

        public PluginPermissionsManager GetPermissionsManager(string connectionString)
        {
            var contextFactory = new WorkflowPnContextFactory();
            var context = contextFactory.CreateDbContext(new[] { connectionString });

            return new PluginPermissionsManager(context);
        }

        private async void SeedWorkOrderForms(IServiceCollection serviceCollection)
        {
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IPluginDbOptions<WorkflowBaseSettings> pluginDbOptions =
                serviceProvider.GetRequiredService<IPluginDbOptions<WorkflowBaseSettings>>();

            Core core = await serviceProvider.GetRequiredService<IEFormCoreService>().GetCore();
            WorkflowPnDbContext context = serviceProvider.GetRequiredService<WorkflowPnDbContext>();

            if (pluginDbOptions.Value.IncidentPlaceListId == 0)
            {
                int incidentPlaceListId = await SeedHelper.CreateAccidentLocationList(core);
                await pluginDbOptions.UpdateDb(settings => settings.IncidentPlaceListId = incidentPlaceListId, context, 1);
            }

            if (pluginDbOptions.Value.IncidentTypeListId == 0)
            {
                int incidentPlaceListId = await SeedHelper.CreateAccidentTypesList(core);
                await pluginDbOptions.UpdateDb(settings => settings.IncidentTypeListId = incidentPlaceListId, context, 1);
            }

            if (pluginDbOptions.Value.FirstEformId == 0)
            {
                int newTaskId = await SeedHelper.CreateNewTaskEform(core);
                await pluginDbOptions.UpdateDb(settings => settings.FirstEformId = newTaskId, context, 1);
            }

            if (pluginDbOptions.Value.SecondEformId == 0)
            {
                int taskListId = await SeedHelper.CreateTaskListEform(core);
                await pluginDbOptions.UpdateDb(settings => settings.SecondEformId = taskListId, context, 1);
            }
        }

    }
}

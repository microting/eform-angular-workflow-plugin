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


using System.Linq;
using System.Runtime.InteropServices;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using eFormCore;
using Microting.eForm.Dto;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.EformAngularFrontendBase.Infrastructure.Data;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
using Microting.eFormWorkflowBase.Helpers;
using QuestPDF.Infrastructure;
using Sentry;
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
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWorkflowLocalizationService, WorkflowLocalizationService>();
            services.AddTransient<IWorkflowPnSettingsService, WorkflowPnSettingsService>();
            services.AddTransient<IWorkflowCasesService, WorkflowCasesService>();
            services.AddTransient<IWorkflowCasesService, WorkflowCasesService>();
            services.AddControllers();
            SeedWorkOrderForms(services);
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void ConfigureOptionsServices(IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigurePluginDbOptions<WorkflowBaseSettings>(
                configuration.GetSection("WorkflowBaseSettings"));
        }

        public void ConfigureDbContext(IServiceCollection services, string connectionString)
        {
            SentrySdk.Init(options =>
            {
                // A Sentry Data Source Name (DSN) is required.
                // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                options.Dsn = "https://80b935299e69fe24c21043140b62f2ab@o4506241219428352.ingest.us.sentry.io/4508266849501184";

                // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                // This might be helpful, or might interfere with the normal operation of your application.
                // We enable it here for demonstration purposes when first trying Sentry.
                // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                options.Debug = false;

                // This option is recommended. It enables Sentry's "Release Health" feature.
                options.AutoSessionTracking = true;

                // This option is recommended for client applications only. It ensures all threads use the same global scope.
                // If you're writing a background service of any kind, you should remove this.
                options.IsGlobalModeEnabled = true;
            });

            string pattern = @"Database=(\d+)_eform-angular-workflow-plugin;";
            Match match = Regex.Match(connectionString!, pattern);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value;
                int number = int.Parse(numberString);
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetTag("customerNo", number.ToString());
                    Console.WriteLine("customerNo: " + number);
                    scope.SetTag("osVersion", Environment.OSVersion.ToString());
                    Console.WriteLine("osVersion: " + Environment.OSVersion);
                    scope.SetTag("osArchitecture", RuntimeInformation.OSArchitecture.ToString());
                    Console.WriteLine("osArchitecture: " + RuntimeInformation.OSArchitecture);
                    scope.SetTag("osName", RuntimeInformation.OSDescription);
                    Console.WriteLine("osName: " + RuntimeInformation.OSDescription);
                });
            }

            _connectionString = connectionString;
            services.AddDbContext<WorkflowPnDbContext>(o => o.UseMySql(connectionString, new MariaDbServerVersion(
                new Version(10, 4, 0)), mySqlOptionsAction: builder =>
            {
                builder.EnableRetryOnFailure();
                builder.MigrationsAssembly(PluginAssembly().FullName);
                builder.TranslateParameterizedCollectionsToConstants();
            }));

            var angularDbConnectionString = connectionString.Replace(
                "eform-angular-workflow-plugin",
                "Angular");
            services.AddDbContext<BaseDbContext>(o => o.UseMySql(angularDbConnectionString, new MariaDbServerVersion(
                new Version(10, 4, 0)), mySqlOptionsAction: builder =>
            {
                builder.EnableRetryOnFailure();
                builder.MigrationsAssembly(PluginAssembly().FullName);
                builder.TranslateParameterizedCollectionsToConstants();
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
                                 Name = "Incidents",
                                 Language = LanguageNames.English,
                            },
                            new PluginMenuTranslationModel
                            {
                                 LocaleName = LocaleNames.German,
                                 Name = "Vorfälle",
                                 Language = LanguageNames.German,
                            },
                            new PluginMenuTranslationModel
                            {
                                 LocaleName = LocaleNames.Danish,
                                 Name = "Hændelser",
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
                                            Name = "Hændelser",
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
                                        Name = "Hændelser",
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
            var sdkDbContext = core.DbContextHelper.GetDbContext();

            // CheckUploadedDataIntegrity(sdkDbContext, core);

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
            else
            {
                var checkList = await sdkDbContext.CheckLists.FirstAsync(x => x.Id == pluginDbOptions.Value.FirstEformId);
                checkList.IsLocked = true;
                checkList.IsEditable = false;
                checkList.IsHidden = true;
                await checkList.Update(sdkDbContext);
            }

            if (pluginDbOptions.Value.SecondEformId == 0)
            {
                int taskListId = await SeedHelper.CreateTaskListEform(core);
                await pluginDbOptions.UpdateDb(settings => settings.SecondEformId = taskListId, context, 1);
            }
            else
            {
                var checkList = await sdkDbContext.CheckLists.FirstAsync(x => x.Id == pluginDbOptions.Value.SecondEformId);
                checkList.IsLocked = true;
                checkList.IsEditable = false;
                checkList.IsHidden = true;
                await checkList.Update(sdkDbContext);
            }

            if (pluginDbOptions.Value.InstructionseFormId == 0)
            {
                int formId = await SeedHelper.CreateInstructioneForm(core);
                await pluginDbOptions.UpdateDb(settings => settings.InstructionseFormId = formId, context, 1);
            }
            else
            {
                var checkList = await sdkDbContext.CheckLists.FirstAsync(x => x.Id == pluginDbOptions.Value.InstructionseFormId);
                checkList.IsLocked = true;
                checkList.IsEditable = false;
                checkList.IsHidden = true;
                await checkList.Update(sdkDbContext);
            }
        }

        private static void CheckUploadedDataIntegrity(MicrotingDbContext dbContext, Core core)
        {
            AmazonS3Client s3Client;
            string s3AccessKeyId = dbContext.Settings.Single(x => x.Name == Settings.s3AccessKeyId.ToString()).Value;
            string s3SecretAccessKey = dbContext.Settings.Single(x => x.Name == Settings.s3SecrectAccessKey.ToString()).Value;
            string s3Endpoint = dbContext.Settings.Single(x => x.Name == Settings.s3Endpoint.ToString()).Value;
            string s3BucktName = dbContext.Settings.Single(x => x.Name == Settings.s3BucketName.ToString()).Value;
            string customerNo = dbContext.Settings.Single(x => x.Name == Settings.customerNo.ToString()).Value;

            if (s3Endpoint.Contains("https"))
            {
                s3Client = new AmazonS3Client(s3AccessKeyId, s3SecretAccessKey, new AmazonS3Config
                {
                    ServiceURL = s3Endpoint,
                });
            }
            else
            {
                s3Client = new AmazonS3Client(s3AccessKeyId, s3SecretAccessKey, RegionEndpoint.EUCentral1);

            }
            var uploadedDatas = dbContext.UploadedDatas.Where(x => x.FileLocation.Contains("https")).ToList();

            foreach (UploadedData ud in uploadedDatas)
            {
                if (ud.FileName == null)
                {
                    core.DownloadUploadedData(ud.Id).GetAwaiter().GetResult();
                }
                else
                {
                    try
                    {
                        GetObjectMetadataRequest request = new GetObjectMetadataRequest
                        {
                            BucketName = $"{s3BucktName}/{customerNo}",
                            Key = ud.FileName
                        };
                        var result = s3Client.GetObjectMetadataAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (AmazonS3Exception s3Exception)
                    {
                        if (s3Exception.ErrorCode == "Forbidden")
                        {
                            core.DownloadUploadedData(ud.Id).GetAwaiter().GetResult();
                        }
                    } catch (Exception ex)
                    {
                        try
                        {
                            GetObjectMetadataRequest request = new GetObjectMetadataRequest
                            {
                                BucketName = s3BucktName,
                                Key = $"{s3BucktName}/{ud.FileName}"
                            };
                            var result = s3Client.GetObjectMetadataAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch (AmazonS3Exception s3Exception)
                        {
                            if (s3Exception.ErrorCode == "Forbidden")
                            {
                                core.DownloadUploadedData(ud.Id).GetAwaiter().GetResult();
                            }
                        }
                    }
                }
            }
        }

    }
}
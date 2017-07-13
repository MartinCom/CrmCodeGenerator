using System.IO;
using CrmCodeGenerator.VSPackage.Model;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public static class ConfigurationFile
    {
        public static Settings ReadFromJsonFile(DTE2 dte2)
        {
            Project project = dte2.GetSelectedProject();

            if (project == null)
            {
                return null;
            }

            var configPath = Path.GetFullPath(Path.Combine(project.GetProjectDirectory(), "codegeneratorconfig.json"));

            if (!File.Exists(configPath))
            {
                string dir = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Status.Update("Adding " + configPath + " to project");

                string templateSamplesPath = Path.Combine(DteHelper.AssemblyDirectory(), @"Resources");

                string defaultTemplatePath = Path.Combine(templateSamplesPath, "codegeneratorconfig.json");

                File.Copy(defaultTemplatePath, configPath, true);

                ProjectItem p = project.ProjectItems.AddFromFile(configPath);
            }

            string configurationJson = System.IO.File.ReadAllText(configPath);

            SettingsModel model = JsonConvert.DeserializeObject<SettingsModel>(configurationJson);

            Settings settings = new Settings
            {
                ServerName = model.ServerName ?? "crm.dynamics.com",
                UseSSL = model.UseSSL,
                UseIFD = model.UseIFD,
                UseOnline = model.UseOnline,
                UseOffice365 = model.UseOffice365,
                ServerPort = model.ServerPort,
                HomeRealm = model.HomeRealm,
                Domain = model.Domain ?? "",
                UseWindowsAuth = model.UseWindowsAuth,
                IsActive = model.IsActive,
                CrmOrg = model.CrmOrg,
                EntitiesToIncludeString = model.EntitiesToIncludeString,
                IncludeNonStandard = model.IncludeNonStandard,
                Dirty = false
            };

            return settings;
        }

        public static void WriteToJsonFile(DTE2 dte2, Settings settings)
        {
            SettingsModel model = new SettingsModel
            {
                Domain = settings.Domain,
                ServerName = settings.ServerName,
                UseIFD = settings.UseIFD,
                UseOnline = settings.UseOnline,
                UseSSL = settings.CanUseSSL,
                EntitiesToIncludeString = settings.EntitiesToIncludeString,
                CrmOrg = settings.CrmOrg,
                HomeRealm = settings.HomeRealm,
                IncludeNonStandard = settings.IncludeNonStandard,
                UseWindowsAuth = settings.UseWindowsAuth
            };

            string jsonConf = JsonConvert.SerializeObject(model);

            Project project = dte2.GetSelectedProject();

            System.IO.File.AppendAllText(System.IO.Path.Combine(project.GetProjectDirectory(), "codegeneratorconfig.json"), jsonConf);
        }
    }
}
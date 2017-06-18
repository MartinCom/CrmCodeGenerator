using CrmCodeGenerator.VSPackage.Model;
using Newtonsoft.Json;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public static class ConfigurationFile
    {
        public static Settings ReadFromJsonFile()
        {
            //todo read from file
            string configurationJson = "{\"UseSSL\":true,\"UseIFD\":false,\"ServerName\":\"servername\",\"UseOnline\":false,\"Domain\":\"defaultdomain\",\"UseOffice365\":false,\"ServerPort\":null,\"HomeRealm\":\"\",\"UseWindowsAuth\":false,\"IsActive\":false,\"CrmOrg\":\"DEV-CRM\",\"EntitiesToIncludeString\":\"account, contact\",\"IncludeNonStandard\":false}";

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

            //settings.Username = pPropBag.Read(_strUsername, "");
            //settings.Password = pPropBag.Read(_strPassword, "");

            return settings;
        }

        public static void CreateDefaultJsonFile()
        {
        }

        public static void WriteToJsonFile(Settings settings)
        {
            SettingsModel model = new SettingsModel();
            model.Domain = "defaultdomain";
            model.ServerName = "servername";
            model.UseIFD = false;
            model.UseOnline = false;
            model.UseSSL = true;
            model.EntitiesToIncludeString = "account, contact";
            model.CrmOrg = "DEV-CRM";
            model.HomeRealm = "";
            model.IncludeNonStandard = false;
            model.UseWindowsAuth = false;

            string jsonConf = JsonConvert.SerializeObject(model);
        }
    }
}
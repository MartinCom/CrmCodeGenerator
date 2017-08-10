using CrmCodeGenerator.VSPackage.Helpers;
using System;

namespace CrmCodeGenerator.VSPackage
{
    public class Configuration
    {
        #region Singleton

        private static Configuration _instance;
        private static readonly Object SyncLock = new Object();

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Configuration();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion Singleton

        public Configuration()
        {
            Settings = new Model.Settings
            {
                CrmSdkUrl = @"https://disco.crm.dynamics.com/XRMServices/2011/Discovery.svc",
                ProjectName = "",
                Domain = "",
                T4Path = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\CrmSvcUtil.tt"),
                Template = "",
                CrmOrg = "DEV-CRM",
                EntitiesToIncludeString = "account,contact,lead,opportunity,systemuser",
                OutputPath = "",
                Username = "@XXXXX.onmicrosoft.com",
                Password = "",
                Namespace = "",
                Dirty = false
            };
        }

        public Model.Settings Settings { get; set; }
        public EnvDTE80.DTE2 DTE { get; set; }
    }
}
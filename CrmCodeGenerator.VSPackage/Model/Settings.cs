﻿using CrmCodeGenerator.VSPackage.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrmCodeGenerator.VSPackage.Model
{
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            EntityList = new ObservableCollection<string>();
            EntitiesSelected = new ObservableCollection<string>();

            Dirty = false;
        }

        #region boiler-plate INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            Dirty = true;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion boiler-plate INotifyPropertyChanged

        private bool _UseSSL;
        private bool _UseIFD;
        private bool _UseOnline;
        private bool _UseOffice365;
        private string _OutputPath;
        private string _Namespace;
        private string _EntitiesToIncludeString;
        private string _CrmOrg;
        private string _Password;
        private string _Username;
        private string _Domain;
        private string _CrmSdkUrl;
        private string _Template;
        private string _T4Path;
        private bool _IncludeNonStandard;
        private bool _IncludeUnpublish;

        private string _ProjectName;

        public string ProjectName
        {
            get => _ProjectName;
            set => SetField(ref _ProjectName, value);
        }

        public string T4Path
        {
            get => _T4Path;
            set => SetField(ref _T4Path, value);
        }

        public string Template
        {
            get => _Template;
            set
            {
                SetField(ref _Template, value);
                NewTemplate = !System.IO.File.Exists(System.IO.Path.Combine(_Folder, _Template));
            }
        }

        private string _Folder = "";

        public string Folder
        {
            get => _Folder;
            set => SetField(ref _Folder, value);
        }

        private bool _NewTemplate;

        public bool NewTemplate
        {
            get => _NewTemplate;
            set => SetField(ref _NewTemplate, value);
        }

        public string OutputPath
        {
            get => _OutputPath;
            set => SetField(ref _OutputPath, value);
        }

        public string CrmSdkUrl
        {
            get => _CrmSdkUrl;
            set => SetField(ref _CrmSdkUrl, value);
        }

        public string Domain
        {
            get => _Domain;
            set => SetField(ref _Domain, value);
        }

        public string Username
        {
            get => _Username;
            set => SetField(ref _Username, value);
        }

        public string Password
        {
            get => _Password;
            set => SetField(ref _Password, value);
        }

        public string CrmOrg
        {
            get => _CrmOrg;
            set => SetField(ref _CrmOrg, value);
        }

        private ObservableCollection<String> _OnLineServers = new ObservableCollection<String>();

        public ObservableCollection<String> OnLineServers
        {
            get => _OnLineServers;
            set => SetField(ref _OnLineServers, value);
        }
        private string _ConnectionString;
        public string ConnectionString
        {
            get => _ConnectionString;
            set => SetField(ref _ConnectionString, value);
        }

        //private string _OnlineServer;
        //public string OnlineServer
        //{
        //    get
        //    {
        //        return _OnlineServer;
        //    }
        //    set
        //    {
        //        SetField(ref _OnlineServer, value);
        //    }
        //}
        private string _ServerName = "";

        public string ServerName
        {
            get => _ServerName;
            set => SetField(ref _ServerName, value);
        }

        private string _ServerPort = "";

        public string ServerPort
        {
            get
            {
                if (UseOnline || UseOffice365)
                {
                    return "";
                }
                return _ServerPort;
            }
            set => SetField(ref _ServerPort, value);
        }

        private string _HomeRealm = "";

        public string HomeRealm
        {
            get => _HomeRealm;
            set => SetField(ref _HomeRealm, value);
        }

        private ObservableCollection<String> _OrgList = new ObservableCollection<String>();

        public ObservableCollection<String> OrgList
        {
            get => _OrgList;
            set => SetField(ref _OrgList, value);
        }

        private ObservableCollection<String> _TemplateList = new ObservableCollection<String>();

        public ObservableCollection<String> TemplateList
        {
            get => _TemplateList;
            set => SetField(ref _TemplateList, value);
        }

        public IOrganizationService CrmConnection { get; set; }

        public string EntitiesToIncludeString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var value in _EntitiesSelected)
                {
                    if (sb.Length != 0)
                        sb.Append(',');
                    sb.Append(value);
                }
                return sb.ToString();
            }
            set
            {
                var newList = new ObservableCollection<string>();
                var split = value.Split(',').Select(p => p.Trim()).ToList();
                foreach (var s in split)
                {
                    newList.Add(s);
                    if (!_EntityList.Contains(s))
                        _EntityList.Add(s);
                }
                EntitiesSelected = newList;
                SetField(ref _EntitiesToIncludeString, value);
                OnPropertyChanged("EnableExclude");
            }
        }

        public ObservableCollection<string> _EntityList;

        public ObservableCollection<string> EntityList
        {
            get => _EntityList;
            set => SetField(ref _EntityList, value);
        }

        public ObservableCollection<string> _EntitiesSelected;

        public ObservableCollection<string> EntitiesSelected
        {
            get => _EntitiesSelected;
            set => SetField(ref _EntitiesSelected, value);
        }

        public string Namespace
        {
            get => _Namespace;
            set => SetField(ref _Namespace, value);
        }

        public bool Dirty { get; set; }

        public bool IncludeNonStandard
        {
            get => _IncludeNonStandard;
            set => SetField(ref _IncludeNonStandard, value);
        }

        public bool IncludeUnpublish
        {
            get => _IncludeUnpublish;
            set => SetField(ref _IncludeUnpublish, value);
        }

        public bool UseSSL
        {
            get => _UseSSL;
            set
            {
                if (SetField(ref _UseSSL, value))
                {
                    ReEvalReadOnly();
                }
            }
        }

        public bool UseIFD
        {
            get => _UseIFD;
            set
            {
                if (SetField(ref _UseIFD, value))
                {
                    if (value)
                    {
                        UseOnline = false;
                        UseOffice365 = false;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }

        public bool UseOnline
        {
            get => _UseOnline;
            set
            {
                if (SetField(ref _UseOnline, value))
                {
                    if (value)
                    {
                        UseIFD = false;
                        UseOffice365 = true;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    else
                    {
                        UseOffice365 = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }

        public bool UseOffice365
        {
            get => _UseOffice365;
            set
            {
                if (SetField(ref _UseOffice365, value))
                {
                    if (value)
                    {
                        UseIFD = false;
                        UseOnline = true;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }

        private bool _UseWindowsAuth;

        public bool UseWindowsAuth
        {
            get => _UseWindowsAuth;
            set
            {
                SetField(ref _UseWindowsAuth, value);
                ReEvalReadOnly();
            }
        }
        private bool _UseConnectionString;

        public bool UseConnectionString
        {
            get => _UseConnectionString;
            set
            {
                SetField(ref _UseConnectionString, value);
                ReEvalReadOnly();
            }
        }

        #region Read Only Properties

        private void ReEvalReadOnly()
        {
            OnPropertyChanged("NeedServer");
            OnPropertyChanged("NeedOnlineServer");
            OnPropertyChanged("NeedServerPort");
            OnPropertyChanged("NeedHomeRealm");
            OnPropertyChanged("NeedCredentials");
            OnPropertyChanged("CanUseWindowsAuth");
            OnPropertyChanged("CanUseSSL");
        }

        public bool NeedServer => !(UseOnline || UseOffice365);

        public bool NeedOnlineServer => (UseOnline || UseOffice365);

        public bool NeedServerPort => !(UseOffice365 || UseOnline);

        public bool NeedHomeRealm => !(UseIFD || UseOffice365 || UseOnline);

        public bool NeedCredentials => !UseWindowsAuth;

        public bool CanUseWindowsAuth => !(UseIFD || UseOnline || UseOffice365);

        public bool CanUseSSL => !(UseOnline || UseOffice365 || UseIFD);

        #endregion Read Only Properties

        #region Conntection Strings

        public AuthenticationProviderType AuthType
        {
            get
            {
                if (UseIFD)
                {
                    return AuthenticationProviderType.Federation;
                }
                else if (UseOffice365)
                {
                    return AuthenticationProviderType.OnlineFederation;
                }
                else if (UseOnline)
                {
                    return AuthenticationProviderType.LiveId;
                }

                return AuthenticationProviderType.ActiveDirectory;
            }
        }

        public string GetDiscoveryCrmConnectionString()
        {
            var connectionString = string.Format("Url={0}://{1}:{2};",
                UseSSL ? "https" : "http",
                UseIFD ? ServerName : UseOffice365 ? "disco." + ServerName : UseOnline ? "dev." + ServerName : ServerName,
                ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort));

            if (!UseWindowsAuth)
            {
                if (!UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        connectionString += string.Format("Domain={0};", Domain);
                    }
                }

                string sUsername = Username;
                if (UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        sUsername = string.Format("{0}\\{1}", Domain, Username);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", sUsername, Password);
            }

            if (UseOnline && !UseOffice365)
            {
                System.ServiceModel.Description.ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);
            }

            if (UseIFD && !string.IsNullOrEmpty(HomeRealm))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealm);
            }

            return connectionString;
        }

        public string GetOrganizationCrmConnectionString()
        {
            if (UseConnectionString)
            {
                return ConnectionString;
            }
            var currentServerName = string.Empty;

            OrganizationDetail orgDetails = ConnectionHelper.GetOrganizationDetails(this);

            if (orgDetails == null)
            {
                throw new NullReferenceException("You must choose organization.");
            }

            if (UseOffice365 || UseOnline)
            {
                currentServerName = string.Format("{0}.{1}", orgDetails.UrlName, ServerName);
            }
            else if (UseIFD)
            {
                var serverNameParts = ServerName.Split('.');

                serverNameParts[0] = orgDetails.UrlName;

                currentServerName = string.Format("{0}:{1}",
                                                  string.Join(".", serverNameParts),
                                                  ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort));
            }
            else
            {
                currentServerName = string.Format("{0}:{1}/{2}",
                                                  ServerName,
                                                  ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort),
                                                  CrmOrg);
            }
            string connectionString = string.Empty;
            if (UseOnline)
            {
                 connectionString = string.Format("AuthType=Office365;Url={0};", orgDetails.Endpoints[EndpointType.WebApplication].TrimEnd('/'));
            }
            else
            {
                connectionString = string.Format("Url={0};", orgDetails.Endpoints[EndpointType.OrganizationService].Replace("/XRMServices/2011/Organization.svc", ""));

            }
            

            if (!UseSSL)
            {
                if (connectionString.Contains("https"))
                {
                    connectionString = connectionString.Replace("https", "http");
                }
            }

            if (!UseWindowsAuth)
            {
                if (!UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        connectionString += string.Format("Domain={0};", Domain);
                    }
                }

                string username = Username;
                if (UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        username = string.Format("{0}\\{1}", Domain, Username);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", username, Password);
            }

            if (UseOnline)
            {
                System.ServiceModel.Description.ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);
            }

            if (UseIFD && !string.IsNullOrEmpty(HomeRealm))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealm);
            }

            //append timeout in seconds to connectionstring
            //connectionString += string.Format("Timeout={0};", Timeout.ToString(@"hh\:mm\:ss"));
            return connectionString;
        }

        #endregion Conntection Strings

        public bool IsActive { get; set; }
    }
}
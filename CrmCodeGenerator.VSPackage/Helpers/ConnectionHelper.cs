using CrmCodeGenerator.VSPackage.Model;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public class ConnectionHelper
    {
        public static OrganizationDetail GetOrganizationDetails(Settings settings)
        {
            var orgs = GetOrganizations(settings);
            var details = orgs.Where(d => d.UrlName == settings.CrmOrg).FirstOrDefault();
            return details;
        }

        public static OrganizationDetailCollection GetOrganizations(Settings settings)
        {
            try
            {
                //System.Windows.MessageBox.Show(typeof(GuidList).Assembly.Location);
                string DiscoveryUrl = string.Format("{0}://{1}:{2}/XRMServices/2011/Discovery.svc",
                settings.UseSSL ? "https" : "http",
                settings.UseIFD ? settings.ServerName : settings.UseOffice365 ? "disco." + settings.ServerName : settings.UseOnline ? "dev." + settings.ServerName : settings.ServerName,
                settings.ServerPort.Length == 0 ? (settings.UseSSL ? 443 : 80) : int.Parse(settings.ServerPort));
                string domain = null;
                string login = settings.Username;
                ClientCredentials deviceCredentials = null;
                Uri homeRealm = null;
                if (!settings.UseWindowsAuth)
                {
                    if (!settings.UseIFD)
                    {
                        if (!string.IsNullOrEmpty(settings.Domain))
                        {
                            domain = settings.Domain;
                            //connectionString += string.Format("Domain={0};", settings.Domain);
                        }
                    }

                    string sUsername = settings.Username;
                    if (settings.UseIFD)
                    {
                        if (!string.IsNullOrEmpty(settings.Domain))
                        {
                            // sUsername = string.Format("{0}\\{1}", settings.Domain, settings.Username);
                            login = string.Format("{0}\\{1}", settings.Domain, settings.Username);
                        }
                    }
                }

                if (settings.UseOnline && !settings.UseOffice365)
                {
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

                    //connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                    //                                  deviceCredentials.UserName.UserName,
                    //                                  deviceCredentials.UserName.Password);
                }

                if (settings.UseIFD && !string.IsNullOrEmpty(settings.HomeRealm))
                {
                    //connectionString += string.Format("HomeRealmUri={0};", settings.HomeRealm);
                    homeRealm = new Uri(settings.HomeRealm);
                }
                NetworkCredential userCredentials;
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    userCredentials = new NetworkCredential(login, settings.Password, domain);
                }
                else if (settings.UseWindowsAuth)
                {
                    userCredentials = CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    userCredentials = new NetworkCredential(login, settings.Password);
                }
                   if(settings.UseOnline && settings.UseOffice365)
                {
                    ClientCredentials client = new ClientCredentials();
                    client.UserName.UserName = login;
                    client.UserName.Password = settings.Password;
                    return CrmServiceClient.DiscoverOrganizations(new Uri(DiscoveryUrl), homeRealm, client,null);
                    //return CrmServiceClient.DiscoverOrganizations(new Uri("https://disco.crm4.dynamics.com/XRMServices/2011/Discovery.svc"), homeRealm, client, null);
                }
                    if (deviceCredentials == null)
                {
                    return CrmServiceClient.DiscoverOrganizations(new Uri(DiscoveryUrl), homeRealm, userCredentials);
                }
                else
                {
                    return CrmServiceClient.DiscoverOrganizations(new Uri(DiscoveryUrl), homeRealm, null, deviceCredentials);
                }

                // var connection = new CrmServiceClient(settings.GetDiscoveryCrmConnectionString());

                //CrmServiceClient.DiscoverOrganizations()

                // var request = new RetrieveOrganizationsRequest();
                // var response = (Microsoft.Xrm.Sdk.Discovery.RetrieveOrganizationsResponse)service.Execute(request);
                // return connection.DiscoverOrganizations();
            }
            catch (System.IO.FileNotFoundException e)
            {
                if (e.Message.Contains("Microsoft.IdentityModel"))
                {
                    throw new Exception("Unable to load Windows Identity Foundation 3.5.  This is a feature that can be enabled on windows 8+ or downloaded for earlier versions ->  https://www.microsoft.com/en-nz/download/details.aspx?id=17331 ", e);
                }
                else
                {
                    throw e;
                }
            }
        }

        public static ObservableCollection<string> GetOrgList(Settings settings)
        {
            var orgs = GetOrganizations(settings);
            var newOrgs = new ObservableCollection<String>(orgs.Select(d => d.UrlName).ToList());
            return newOrgs;
        }
    }
}
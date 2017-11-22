using System.Runtime.Serialization;

namespace CrmCodeGenerator.VSPackage.Model
{
    [DataContract]
    public class SettingsModel
    {
        [DataMember]
        public bool UseSSL { get; set; }

        [DataMember]
        public bool UseIFD { get; set; }

        [DataMember]
        public string ServerName { get; set; }

        [DataMember]
        public bool UseOnline { get; set; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public bool UseOffice365 { get; set; }

        [DataMember]
        public string ServerPort { get; set; }

        [DataMember]
        public string HomeRealm { get; set; }

        [DataMember]
        public bool UseWindowsAuth { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string CrmOrg { get; set; }

        [DataMember]
        public string EntitiesToIncludeString { get; set; }

        [DataMember]
        public bool IncludeNonStandard { get; set; }
    }
}
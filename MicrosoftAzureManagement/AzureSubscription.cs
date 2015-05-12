using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftAzureManagement
{
    public class AzureSubscription
    {
        public AzureSubscription(string name, string id, string managementCertificate)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Subscription Name or ID is null or empty.");
            }
            Name = name;
            Id = id;
            ManagementCertificate = new X509Certificate2(
                        Convert.FromBase64String(managementCertificate));
        }
        public string Name{ private set; get; }
        public string Id { private set; get; }
        public X509Certificate2 ManagementCertificate { private set; get; }

    }
}

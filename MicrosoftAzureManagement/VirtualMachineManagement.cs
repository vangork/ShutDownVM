using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MicrosoftAzureManagement
{
    public class VirtualMachine
    {
        public string Service { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string OS { get; set; }

    }
    public class VirtualMachineManagement : AzureManagement
    {
        public VirtualMachineManagement(AzureSubscription subscription, PropertyInvoker log = null)
            : base(subscription, log)
        { }

        public HttpStatusCode Shutdown(string serviceName, string deploymentName, string serverName)
        {
            //https://management.core.windows.net/<subscription-id>/services/hostedservices/<cloudservice-name>/deployments/<deployment-name>/roleinstances/<role-name>/Operations
            //POST

            Uri shutdownUri = new Uri(string.Format("{0}/services/hostedservices/{1}/deployments/{2}/roleinstances/{3}/Operations",
                AzureManagementUri, serviceName, deploymentName, serverName));

            XNamespace wa = "http://schemas.microsoft.com/windowsazure";
            // Create the request to add a new Virtual Machine deployment
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XDocument requestBody = new XDocument(
              new XDeclaration("1.0", "UTF-8", "no"),
              new XElement(wa + "ShutdownRoleOperation",
                new XElement(wa + "OperationType", "ShutdownRoleOperation"),
                new XElement(wa + "PostShutdownAction", "StoppedDeallocated")));

            // Submit the request and get the response
            HttpWebResponse response = base.InvokeRequest(shutdownUri, "POST", requestBody);
            HttpStatusCode statusCode = response.StatusCode;
            return statusCode;
        }

        public HttpStatusCode Get(string serviceName, string deploymentName, string serverName, out XDocument responseBody)
        {
            //https://msdn.microsoft.com/en-us/library/azure/jj157193.aspx
            //https://management.core.windows.net/<subscription-id>/services/hostedservices/<cloudservice-name>/deployments/<deployment-name>/roles/<role-name>
            //GET

            Uri getUri = new Uri(string.Format("{0}/services/hostedservices/{1}/deployments/{2}/roles/{3}",
                AzureManagementUri, serviceName, deploymentName, serverName));

            Log += string.Format("Get-AzureVM ing...\n");

            // Submit the request and get the response
            //XDocument responseBody;
            HttpWebResponse response = InvokeRequest(getUri, "GET", out responseBody);
            Log += string.Format("Get-AzureVM Done\n");

            HttpStatusCode statusCode = response.StatusCode;
            return statusCode;
        }


        public List<VirtualMachine> List()
        {
            CloudServiceManagement cloudServiceManagement = new CloudServiceManagement(Subscription, Log);
            XDocument cloudServices;
            HttpStatusCode status = cloudServiceManagement.List(out cloudServices);
            if (status != HttpStatusCode.OK)
                return null;

            XNamespace ns = cloudServices.Root.Name.Namespace;
            var cloudServiceList = from p in cloudServices.Descendants()
                                   where p.Name.LocalName == "ServiceName"
                                   select p;
            Log += string.Format("{0} Service Fetched!\n", cloudServiceList.Count());

            List<VirtualMachine> azureVMList = new List<VirtualMachine>();
            foreach (var p in cloudServiceList)
            {
                status = cloudServiceManagement.GetDeployment(p.Value, out cloudServices);
                if (status != HttpStatusCode.OK)
                {
                    Log += string.Format("No VM under Service:{0}\n", p.Value.ToString());
                    continue;
                }

                var vms = from el in cloudServices.Descendants(ns + "RoleInstance")
                          join els in cloudServices.Descendants(ns + "Role")
                          on el.Element(ns + "RoleName").Value equals els.Element(ns + "RoleName").Value
                          select new VirtualMachine
                          {
                              Service = p.Value,
                              Name = (string)el.Element(ns + "InstanceName").Value,
                              Status = (string)el.Element(ns + "InstanceStatus").Value,
                              OS = (string)els.Element(ns + "OSVirtualHardDisk").Element(ns + "OS").Value
                          };
                Log += string.Format("{0} VMs found under Service:{1}\n", vms.Count(), p.Value.ToString());
                azureVMList.AddRange(vms);
            }
            return azureVMList.Count > 0 ? azureVMList : null;

        }
    }
}

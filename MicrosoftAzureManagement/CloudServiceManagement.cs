using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MicrosoftAzureManagement
{
    public class CloudServiceManagement : AzureManagement
    {
        public CloudServiceManagement(AzureSubscription subscription, PropertyInvoker log = null)
            : base(subscription, log)
        { }

        public HttpStatusCode List(out XDocument responseBody)
        {
            //https://msdn.microsoft.com/en-us/library/azure/ee460781.aspx
            //https://management.core.windows.net/<subscription-id>/services/hostedservices
            //GET

            Uri listUri = new Uri(string.Format("{0}/services/hostedservices",
                AzureManagementUri));

            Log += string.Format("Get-AzureService ing...\n");

            // Submit the request and get the response
            //XDocument responseBody;
            HttpWebResponse response = InvokeRequest(listUri, "GET", out responseBody);
            Log += string.Format("Get-AzureService Done\n");
           
            HttpStatusCode statusCode = response.StatusCode;
            return statusCode;
            
            //Console.WriteLine("The status of the operation: {0}\n\n", statusCode.ToString());
            //Console.WriteLine(responseBody.ToString(SaveOptions.OmitDuplicateNamespaces));
        }

        public HttpStatusCode GetDeployment(string serviceName, out XDocument responseBody)
        {
            //https://msdn.microsoft.com/en-us/library/azure/ee460804.aspx
            //https://management.core.windows.net/<subscription-id>/services/hostedservices/<cloudservice-name>/deploymentslots/<deployment-slot>
            //GET

            Uri GetDeploymentUri = new Uri(string.Format("{0}/services/hostedservices/{1}/deploymentslots/Production",
                AzureManagementUri, serviceName));

            Log += string.Format("Get-AzureDeployment ing...\n");
            
            // Submit the request and get the response
            //XDocument responseBody;
            HttpWebResponse response = InvokeRequest(GetDeploymentUri, "GET", out responseBody);
            Log += string.Format("Get-AzureDeployment Done\n");

            HttpStatusCode statusCode = response.StatusCode;
            return statusCode;
            //Console.WriteLine("The status of the operation: {0}\n\n", statusCode.ToString());
            //Console.WriteLine(responseBody.ToString(SaveOptions.OmitDuplicateNamespaces));
        
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MicrosoftAzureManagement
{
    public class AzureManagement
    {
        private const string VERSION = "2014-10-01";
        private const string SERVICE_MANAGEMENT_URL = "https://management.core.windows.net";
        public AzureSubscription Subscription { get; set; }
        public PropertyInvoker Log { get; set; }
        public string AzureManagementUri { get { return SERVICE_MANAGEMENT_URL + "/" + Subscription.Id; } }

        public AzureManagement(AzureSubscription subscription, PropertyInvoker log = null)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException(
                    "Subscription", "Subscription parameter cannot be null.");
            }
            Subscription = subscription;
            Log = log;
        }


        public HttpWebResponse InvokeRequest(Uri uri, string method, XDocument requestBody)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = method;
            request.Headers.Add("x-ms-version", VERSION);
            request.ClientCertificates.Add(Subscription.ManagementCertificate);
            request.ContentType = "application/xml";

            if (requestBody != null)
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(
                      requestStream, System.Text.UTF8Encoding.UTF8))
                    {
                        requestBody.Save(streamWriter, SaveOptions.DisableFormatting);
                    }
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            response.Close();
            return response;
        }

        public HttpWebResponse InvokeRequest(Uri uri, string method, out XDocument responseBody)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = method;
            request.Headers.Add("x-ms-version", VERSION);
            request.ClientCertificates.Add(Subscription.ManagementCertificate);
            request.ContentType = "application/xml";

            responseBody = null;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            if (response.ContentLength > 0)
            {
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream(), settings))
                {
                    try
                    {
                        responseBody = XDocument.Load(reader);
                    }
                    catch
                    {
                        responseBody = null;
                    }
                }
            }
            response.Close();
            return response;
        }

    }
}

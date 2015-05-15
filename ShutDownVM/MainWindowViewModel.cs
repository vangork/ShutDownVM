using MicrosoftAzureManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShutDownVM
{

    public class MainWindowViewModel : BindableBase
    {
        private string serverName;
        private ObservableCollection<string> serviceNames;
        private string selectServiceName;
        private string serviceName;
        private string outputLog;
        private bool isEditableOn;
        private bool isShutDownEnable;
        private bool isInputEnable;
        private string checkBTNText;
        private Thread thread;
        private AzureSubscription subscription;
        //private List<VirtualMachine> vmList;

        public string ServerName
        {
            get { return serverName; }
            set
            {
                serverName = value;
                OnPropertyChanged();

                // set ShutDown button disabled && ComboBox Editable enabled
                IsShutDownEnable = false;
                ServiceNames.Clear();
                ServiceName = null;

            }
        }
        public string SelectServiceName
        {
            get { return selectServiceName; }
            set
            {
                selectServiceName = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> ServiceNames
        {
            get { return serviceNames; }
            set
            {
                serviceNames = value;
                OnPropertyChanged();
            }
        }
        public string ServiceName
        {
            get { return serviceName; }
            set
            {
                serviceName = value;
                OnPropertyChanged();
            }
        }
        public string OutputLog
        {
            get { return outputLog; }
            set
            {
                outputLog = value;
                OnPropertyChanged();
            }
        }
        public bool IsEditableOn
        {
            get { return isEditableOn; }
            set
            {
                isEditableOn = value;
                OnPropertyChanged();
            }
        }
        public bool IsShutDownEnable
        {
            get { return isShutDownEnable; }
            set
            {
                isShutDownEnable = value;
                OnPropertyChanged();
            }
        }
        public bool IsInputEnable
        {
            get { return isInputEnable; }
            set
            {
                isInputEnable = value;
                OnPropertyChanged();
            }
        }
        public string CheckBTNText
        {
            get { return checkBTNText; }
            set
            {
                checkBTNText = value;
                OnPropertyChanged();
            }
        }

        private VirtualMachineManagement azureVmManagement;

        public MainWindowViewModel()
        {
            // initial subscription
            using (var fs = File.OpenRead("./subscription.publishsettings"))
            {
                var document = XDocument.Load(fs);

                var subscriptions = document.Descendants("Subscription");

                foreach (var p in subscriptions)
                {
                    subscription = new AzureSubscription(p.Attribute("Name").Value, p.Attribute("Id").Value, p.Attribute("ManagementCertificate").Value);
                }
            }
            azureVmManagement = new VirtualMachineManagement(subscription, new PropertyInvoker("OutputLog", this));

            // initialize ServerName textbox and Check Bottun Text
            ServiceNames = new ObservableCollection<string>();
            ServerName = System.Environment.MachineName.ToString();

            IsShutDownEnable = false;
            isEditableOn = true;
            IsInputEnable = true;

            CheckBTNText = "Check";
            OutputLog = "Welcome to Microsoft Azure Management Tool!\n";
            OutputLog += string.Format("Current Server Name is: {0}\n", ServerName);
        }

        public ActionCommand CheckCommand
        {
            get { return (Action)Check; }
        }
        private void Check()
        {
            if (CheckBTNText == "Check")
            {
                CheckBTNText = "Cancel";
                IsInputEnable = false;
                ServiceNames.Clear();

                List<VirtualMachine> vmList = null;

                if (ServiceName == null || ServiceName.Length == 0)
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        thread = Thread.CurrentThread;
                        vmList = null;
                        vmList = azureVmManagement.List();
                                               
                    }).ContinueWith(s=>
                    {
                        if (vmList != null && s.Status == TaskStatus.RanToCompletion)
                        {
                            foreach (var p in vmList)
                            {
                                if (p.Name.ToLower() == ServerName.ToLower())
                                    ServiceNames.Add(p.Service);

                            }

                            if (ServiceNames.Count > 0)
                            {
                                ServiceNames.Add("test");
                                IsShutDownEnable = true;
                                OutputLog += string.Format("Result: {0} Cloud Service found hosting server: {1}\nPlease Select!\n", ServiceNames.Count, ServerName);
                                ServiceName = ServiceNames[0];
                                
                            }
                            else
                            {
                                OutputLog += string.Format("Result: No Cloud Service host server: {0}\nPlease check again!\n", ServerName);
                            }
                        }

                        CheckBTNText = "Check";
                        IsInputEnable = true;
                        
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    HttpStatusCode status = 0;
                    Task task = Task.Factory.StartNew(() =>
                    {
                        thread = Thread.CurrentThread;
                        XDocument vm;
                        status = azureVmManagement.Get(ServiceName, ServiceName, ServerName, out vm);
                    }).ContinueWith(s=>
                    {
                        if (status == HttpStatusCode.OK && s.Status == TaskStatus.RanToCompletion)
                        {
                            IsShutDownEnable = true;
                            OutputLog += string.Format("Result: Verified!\nPlease click on the Shut Down button to proceed...\n");
                        }
                        else
                        {

                            OutputLog += string.Format("Result: Cloud Service or Server is not available!\nPlease check again!\n");
                        }

                        CheckBTNText = "Check";
                        IsInputEnable = true;
                    },TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            else
            {
                thread.Abort();
                OutputLog += string.Format("Check canceled\n");
            }

        }

        public ActionCommand ShutdownCommand
        {
            get { return (Action)Shutdown; }
        }
        private void Shutdown()
        {
            //var status = AzureVmManagement.Shutdown(CloudServiceName, CloudServiceName, VmName);
            string aa = ServerName;
            string bb = ServiceName;

        }

    }
}

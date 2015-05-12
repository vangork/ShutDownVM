﻿using MicrosoftAzureManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShutDownVM
{

    public class MainWindowViewModel : BindableBase
    {
        private string serverName;
        private string selectServiceName;
        private ObservableCollection<string> serviceName;
        private string outputLog;
        private bool isEditableOn;
        private bool isShutDownEnable;
        private string checkBTNText;
        private List<VirtualMachine> vmList;
        private Thread thread;
        private AzureSubscription subscription;

        public string ServerName
        {
            get { return serverName; }
            set
            {
                serverName = value;
                OnPropertyChanged();

                // set ShutDown button disabled && ComboBox Editable enabled
                IsShutDownEnable = false;
                isEditableOn = false;
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
        public string NewService { set; get; }

        public ObservableCollection<string> ServiceName
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
        public string CheckBTNText
        {
            get { return checkBTNText; }
            set
            {
                checkBTNText = value;
                OnPropertyChanged();
            }
        }

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

            // initialize ServerName textbox and Check Bottun Text
            ServerName = System.Environment.MachineName.ToString();
            CheckBTNText = "Check";

            isEditableOn = true;
            IsShutDownEnable = false;

            ServiceName = new ObservableCollection<string>();
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

                var AzureVmManagement = new VirtualMachineManagement(subscription, new PropertyInvoker("OutputLog", this));

                if (NewService == null || NewService.Length == 0)
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        vmList = null;
                        thread = Thread.CurrentThread;
                        vmList = AzureVmManagement.List();

                    }).ContinueWith(s =>
                        {
                            CheckBTNText = "Check";

                            if (vmList != null && s.Status == TaskStatus.RanToCompletion)
                            {
                                foreach (var p in vmList)
                                {
                                    if (p.Name.ToLower() == ServerName.ToLower())
                                        ServiceName.Add(p.Service);
                                   
                                }
                                if (ServiceName.Count > 0)
                                {
                                    OutputLog += string.Format("Result: {0} Cloud Service found hosting server: {1}\nPlease Select!\n", ServiceName.Count, ServerName);

                                    SelectServiceName = ServiceName[0];
                                    IsShutDownEnable = true;


                                }
                                else
                                {
                                    OutputLog += string.Format("Result: No Cloud Service host server: {0}\nPlease check again!\n", ServerName);
                                }
                            }

                        }, TaskScheduler.FromCurrentSynchronizationContext());

                }
                else
                {
                    string aa = ServerName;
                    string bb = NewService;
                }
            }
            else
            {
                thread.Abort();
                OutputLog += string.Format("Check canceled\n");

                CheckBTNText = "Check";
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
            string bb = SelectServiceName;
        }

    }
}
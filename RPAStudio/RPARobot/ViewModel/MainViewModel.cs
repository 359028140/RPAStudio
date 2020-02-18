using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using NuGet;
using System.Linq;
using RPARobot.Librarys;
using System.Collections.ObjectModel;
using log4net;
using System.Collections.Generic;
using RPARobot.Executor;
using GalaSoft.MvvmLight.Messaging;
using System;
using RPARobot.Services;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Flurl.Http;
using Plugins.Shared.Library;
using System.Threading;

namespace RPARobot.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public RunManager m_runManager { get; set; }

        public Window m_view { get; set; }

        public string PackagesDir { get; set; }

        public string InstalledPackagesDir { get; set; }

        /// <summary>
        /// ffmpeg������
        /// </summary>
        public FFmpegService FFmpegService { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public PackageService PackageService { get; set; }

        /// <summary>
        /// �������ķ�����
        /// </summary>
        public ControlServerService ControlServerService { get; set; }

        /// <summary>
        /// ע�ᶨʱ��
        /// </summary>
        private DispatcherTimer RegisterTimer { get; set; }

        /// <summary>
        /// ��ȡ��������������������̶�ʱ��
        /// </summary>
        private DispatcherTimer GetProcessesTimer { get; set; }

        /// <summary>
        /// ��ȡ��ǰӦ�����е����̵Ķ�ʱ��
        /// </summary>
        private DispatcherTimer GetRunProcesssTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            var commonApplicationData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            PackagesDir = commonApplicationData + @"\RPAStudio\Packages";//������Ĭ�϶�ȡnupkg����λ��
            InstalledPackagesDir = commonApplicationData + @"\RPAStudio\InstalledPackages";//������Ĭ�ϰ�װnupkg����λ��
            if(PackageService == null)
            {
                PackageService = new PackageService(this);
            }

            Messenger.Default.Register<RunManager>(this, "BeginRun", BeginRun);
            Messenger.Default.Register<RunManager>(this, "EndRun", EndRun);
        }

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        public void InitControlServer()
        {
            if (ControlServerService == null)
            {
                ControlServerService = new ControlServerService();
            }

            RegisterTimer = new DispatcherTimer();
            RegisterTimer.Interval = TimeSpan.FromSeconds(60);
            RegisterTimer.Tick += RegisterTimer_Tick;
            RegisterTimer.Start();
            RegisterTimer_Tick(null, null);

            GetProcessesTimer = new DispatcherTimer();
            GetProcessesTimer.Interval = TimeSpan.FromSeconds(30);
            GetProcessesTimer.Tick += GetProcessesTimer_Tick;
            GetProcessesTimer.Start();
            GetProcessesTimer_Tick(null, null);

            //GetRunProcesssTimer
            GetRunProcesssTimer = new DispatcherTimer();
            GetRunProcesssTimer.Interval = TimeSpan.FromSeconds(30);
            GetRunProcesssTimer.Tick += GetRunProcesssTimer_Tick;
            GetRunProcesssTimer.Start();
            GetRunProcesssTimer_Tick(null, null);

            //TEST
            //ControlServerService.UpdateRunStatus("�������̷����ڶ���", "2.0.8", ControlServerService.enProcessStatus.Exception);
            //ControlServerService.Log("test1","2.0.1","DEBUG","������־22222");
        }

       /// <summary>
       /// ע�ᶨʱ��������
       /// </summary>
       /// <param name="sender">������</param>
       /// <param name="e">����</param>
        private void RegisterTimer_Tick(object sender, EventArgs e)
        {
            ControlServerService.Register();
        }

        /// <summary>
        /// ��ȡ�����б�ʱ��������
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="e">����</param>
        private void GetProcessesTimer_Tick(object sender, EventArgs e)
        {
            GetProcessesTimer.Stop();

            Task.Run(async()=> {
                var jArr = await ControlServerService.GetProcesses();

                //���ذ�װ��
                if(jArr != null)
                {
                    bool needRefresh = false;
                    for (int i = 0; i < jArr.Count; i++)
                    {
                        var jObj = jArr[i];
                        var nupkgName = jObj["PROCESSNAME"].ToString();
                        var nupkgVersion = jObj["PROCESSVERSION"].ToString();
                        var nupkgFileName = jObj["NUPKGFILENAME"].ToString();
                        var nupkgUrl = jObj["NUPKGURL"].ToString();

                        //�жϱ����Ƿ���ڸð�������������������
                        if(!System.IO.File.Exists(System.IO.Path.Combine(PackagesDir, nupkgFileName)))
                        {
                            var downloadAndSavePath = await nupkgUrl.DownloadFileAsync(PackagesDir, nupkgFileName);
                            needRefresh = true;
                        }

                        //�ȵ�ǰ���汾�ߵ�ȫɾ����RobotĬ��ֻ�����и߰汾�ģ����Բ�ɾ���ᵼ����������ͻ��
                        var repo = PackageRepositoryFactory.Default.CreateRepository(PackagesDir);
                        var pkgNameList = repo.FindPackagesById(nupkgName);
                        foreach (var item in pkgNameList)
                        {
                            if(item.Version > new SemanticVersion(nupkgVersion))
                            {
                                //ɾ���ð�
                                var file = PackagesDir + @"\" + nupkgName + @"." + item.Version.ToString() + ".nupkg";
                                Common.DeleteFile(file);
                            }
                        }
                    }

                    if(needRefresh)
                    {
                        Common.RunInUI(() =>
                        {
                            RefreshAllPackages();
                        });
                    }
                }

                GetProcessesTimer.Start();
            });
            
        }

        /// <summary>
        /// ��ȡ��ǰӦ���е����̶�ʱ����������
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="e">����</param>
        private void GetRunProcesssTimer_Tick(object sender, EventArgs e)
        {
            GetRunProcesssTimer.Stop();

            Task.Run(async () =>
            {
                var jObj = await ControlServerService.GetRunProcess();
                if(jObj != null)
                {
                    var processName = jObj["PROCESSNAME"].ToString();
                    var processVersion = jObj["PROCESSVERSION"].ToString();

                    //ֹͣ���������������е�����,����������̣�����������Ѿ��������򲻲�����
                    PackageService.Run(processName, processVersion);
                }

                GetRunProcesssTimer.Start();
            });

        }

        /// <summary>
        /// ��ʼִ�й�����ʱ����
        /// </summary>
        /// <param name="obj">����</param>
        private void BeginRun(RunManager obj)
        {
            SharedObject.Instance.Output(SharedObject.enOutputType.Trace, "�������п�ʼ����");

            //��Ӧ�ø���״̬ΪSTART����Ȼ���û��Լ����õı�ʶ��ͻ��
            //Task.Run(async()=> {
            //    await ControlServerService.UpdateRunStatus(obj.m_packageItem.Name, obj.m_packageItem.Version, ControlServerService.enProcessStatus.Start);
            //});


            if (FFmpegService != null)
            {
                FFmpegService.StopCaptureScreen();
                FFmpegService = null;
            }

            if(ViewModelLocator.Instance.UserPreferences.IsEnableScreenRecorder)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Trace, "��Ļ¼��ʼ����");
                var screenRecorderFilePath = App.LocalRPAStudioDir + @"\ScreenRecorder\" + obj.m_packageItem.Name + @"(" + DateTime.Now.ToString("yyyy��MM��dd��HHʱmm��ss��") + ").mp4";
                FFmpegService = new FFmpegService(screenRecorderFilePath, ViewModelLocator.Instance.UserPreferences.FPS, ViewModelLocator.Instance.UserPreferences.Quality);//Ĭ�ϴ浽%localappdata%��RPASTUDIO�µ�ScreenRecorderĿ¼��

                Task.Run(() =>
                {
                    FFmpegService.StartCaptureScreen();
                });

                //�ȴ���Ļ¼��ffmpeg��������
                int wait_count = 0;
                while(!FFmpegService.IsRunning())
                {
                    wait_count++;
                    Thread.Sleep(300);
                    if(wait_count == 10)
                    {
                        break;
                    }
                }
            }
            

            Common.RunInUI(()=> {
                m_view.Hide();

                obj.m_packageItem.IsRunning = true;

                IsWorkflowRunning = true;
                WorkflowRunningName = obj.m_packageItem.Name;
                WorkflowRunningToolTip = obj.m_packageItem.ToolTip;
                WorkflowRunningStatus = "��������";
            });
        }

        private void EndRun(RunManager obj)
        {
            SharedObject.Instance.Output(SharedObject.enOutputType.Trace, "�������н���");

            Task.Run(async () =>
            {
                if(obj.HasException)
                {
                    await ControlServerService.UpdateRunStatus(obj.m_packageItem.Name, obj.m_packageItem.Version, ControlServerService.enProcessStatus.Exception);
                }
                else
                {
                    await ControlServerService.UpdateRunStatus(obj.m_packageItem.Name, obj.m_packageItem.Version, ControlServerService.enProcessStatus.Stop);
                }
                
            });

            if (ViewModelLocator.Instance.UserPreferences.IsEnableScreenRecorder)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Trace, "��Ļ¼�����");
                FFmpegService.StopCaptureScreen();
                FFmpegService = null;
            }

            Common.RunInUI(() => {
                m_view.Show();
                m_view.Activate();

                obj.m_packageItem.IsRunning = false;

                //�����п����б��Ѿ�ˢ�£�������Ҫ����IsRunning״̬��Ϊ�˷��㣬ȫ������
                foreach (var pkg in PackageItems)
                {
                    pkg.IsRunning = false;
                }

                IsWorkflowRunning = false;
                WorkflowRunningName = "";
                WorkflowRunningStatus = "";
            });
        }


        public void RefreshAllPackages()
        {
            PackageItems.Clear();

            var repo = PackageRepositoryFactory.Default.CreateRepository(PackagesDir);
            var pkgList = repo.GetPackages();

            var pkgSet = new SortedSet<string>();
            foreach (var pkg in pkgList)
            {
                //ͨ��setȥ��
                pkgSet.Add(pkg.Id);
            }

            Dictionary<string, IPackage> installedPkgDict = new Dictionary<string, IPackage>();

            var packageManager = new PackageManager(repo, InstalledPackagesDir);
            foreach (IPackage pkg in packageManager.LocalRepository.GetPackages())
            {
                installedPkgDict[pkg.Id] = pkg;
            }

            foreach (var name in pkgSet)
            {
                var item = new PackageItem();
                item.Name = name;

                var version = repo.FindPackagesById(name).Max(p => p.Version);
                item.Version = version.ToString();

                var pkgNameList = repo.FindPackagesById(name);
                foreach(var i in pkgNameList)
                {
                    item.VersionList.Add(i.Version.ToString());
                }

                bool isNeedUpdate = false;
                if(installedPkgDict.ContainsKey(item.Name))
                {
                    var installedVer = installedPkgDict[item.Name].Version;
                    if(version > installedVer)
                    {
                        isNeedUpdate = true;
                    }
                }
                else
                {
                    isNeedUpdate = true;
                }
                item.IsNeedUpdate = isNeedUpdate;

                var pkg = repo.FindPackage(name, version);
                item.Package = pkg;
                var publishedTime = pkg.Published.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                item.ToolTip = string.Format("���ƣ�{0}\r\n�汾��{1}\r\n����˵����{2}\r\n��Ŀ������{3}\r\n����ʱ�䣺{4}", item.Name, item.Version,pkg.ReleaseNotes,pkg.Description, (publishedTime == null? "δ֪": publishedTime));

                if(IsWorkflowRunning && item.Name == WorkflowRunningName)
                {
                    item.IsRunning = true;//�����ǰ�ð������Ѿ������У���Ҫ����IsRunning
                }

                PackageItems.Add(item);
            }
            

            doSearch();
        }

        private RelayCommand<RoutedEventArgs> _loadedCommand;

        /// <summary>
        /// Gets the LoadedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_view = (Window)p.Source;
                        RefreshAllPackages();
                    }));
            }
        }




        public bool IsEnableAuthorizationCheck
        {
            get
            {
#if ENABLE_AUTHORIZATION_CHECK
                return true;
#else
                return false;
#endif
            }
        }



        private RelayCommand _MouseLeftButtonDownCommand;

        /// <summary>
        /// Gets the MouseLeftButtonDownCommand.
        /// </summary>
        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                return _MouseLeftButtonDownCommand
                    ?? (_MouseLeftButtonDownCommand = new RelayCommand(
                    () =>
                    {
                        //�������Ĳ���Ҳ���϶�������ʹ��
                        m_view.DragMove();
                    }));
            }
        }

        private RelayCommand _activatedCommand;

        /// <summary>
        /// Gets the ActivatedCommand.
        /// </summary>
        public RelayCommand ActivatedCommand
        {
            get
            {
                return _activatedCommand
                    ?? (_activatedCommand = new RelayCommand(
                    () =>
                    {
                        m_view.WindowState = WindowState.Normal;
                        RefreshAllPackages();
                    }));
            }
        }



        private RelayCommand<System.ComponentModel.CancelEventArgs> _closingCommand;

        /// <summary>
        /// Gets the ClosingCommand.
        /// </summary>
        public RelayCommand<System.ComponentModel.CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<System.ComponentModel.CancelEventArgs>(
                    e =>
                    {
                        e.Cancel = true;//���رմ���
                        m_view.Hide();
                    }));
            }
        }




        private RelayCommand _refreshCommand;

        /// <summary>
        /// Gets the RefreshCommand.
        /// </summary>
        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                    () =>
                    {
                        RefreshAllPackages();
                    }));
            }
        }






        private RelayCommand _userPreferencesCommand;

        /// <summary>
        /// Gets the UserPreferencesCommand.
        /// </summary>
        public RelayCommand UserPreferencesCommand
        {
            get
            {
                return _userPreferencesCommand
                    ?? (_userPreferencesCommand = new RelayCommand(
                    () =>
                    {
                        if(!ViewModelLocator.Instance.Startup.UserPreferencesWindow.IsVisible)
                        {
                            var vm = ViewModelLocator.Instance.Startup.UserPreferencesWindow.DataContext as UserPreferencesViewModel;
                            vm.LoadSettings();

                            ViewModelLocator.Instance.Startup.UserPreferencesWindow.Show();
                        }

                        ViewModelLocator.Instance.Startup.UserPreferencesWindow.Activate();
                    }));
            }
        }


        private RelayCommand _viewLogsCommand;

        /// <summary>
        /// Gets the ViewLogsCommand.
        /// </summary>
        public RelayCommand ViewLogsCommand
        {
            get
            {
                return _viewLogsCommand
                    ?? (_viewLogsCommand = new RelayCommand(
                    () =>
                    {
                        //����־���ڵ�Ŀ¼
                        Common.LocateDirInExplorer(App.LocalRPAStudioDir + @"\Logs");
                    }));
            }
        }



        private RelayCommand _viewScreenRecordersCommand;

        /// <summary>
        /// �鿴¼��
        /// </summary>
        public RelayCommand ViewScreenRecordersCommand
        {
            get
            {
                return _viewScreenRecordersCommand
                    ?? (_viewScreenRecordersCommand = new RelayCommand(
                    () =>
                    {
                        //����Ļ¼�����ڵ�Ŀ¼
                        Common.LocateDirInExplorer(App.LocalRPAStudioDir + @"\ScreenRecorder");
                    }));
            }
        }





        private RelayCommand _registerProductCommand;

        /// <summary>
        /// Gets the RegisterProductCommand.
        /// </summary>
        public RelayCommand RegisterProductCommand
        {
            get
            {
                return _registerProductCommand
                    ?? (_registerProductCommand = new RelayCommand(
                    () =>
                    {
                        //����ע�ᴰ��
                        if (!ViewModelLocator.Instance.Startup.RegisterWindow.IsVisible)
                        {
                            var vm = ViewModelLocator.Instance.Startup.RegisterWindow.DataContext as RegisterViewModel;
                            vm.LoadRegisterInfo();

                            ViewModelLocator.Instance.Startup.RegisterWindow.Show();
                        }

                        ViewModelLocator.Instance.Startup.RegisterWindow.Activate();
                    }));
            }
        }



        private RelayCommand _aboutProductCommand;

        /// <summary>
        /// ���ڲ�Ʒ����
        /// </summary>
        public RelayCommand AboutProductCommand
        {
            get
            {
                return _aboutProductCommand
                    ?? (_aboutProductCommand = new RelayCommand(
                    () =>
                    {
                        if (!ViewModelLocator.Instance.Startup.AboutWindow.IsVisible)
                        {
                            var vm = ViewModelLocator.Instance.Startup.AboutWindow.DataContext as AboutViewModel;
                            vm.LoadAboutInfo();

                            ViewModelLocator.Instance.Startup.AboutWindow.Show();
                        }

                        ViewModelLocator.Instance.Startup.AboutWindow.Activate();
                    }));
            }
        }





        /// <summary>
        /// The <see cref="PackageItems" /> property's name.
        /// </summary>
        public const string PackageItemsPropertyName = "PackageItems";

        private ObservableCollection<PackageItem> _packageItemsProperty = new ObservableCollection<PackageItem>();

        /// <summary>
        /// Sets and gets the PackageItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PackageItem> PackageItems
        {
            get
            {
                return _packageItemsProperty;
            }

            set
            {
                if (_packageItemsProperty == value)
                {
                    return;
                }

                _packageItemsProperty = value;
                RaisePropertyChanged(PackageItemsPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsSearchResultEmpty" /> property's name.
        /// </summary>
        public const string IsSearchResultEmptyPropertyName = "IsSearchResultEmpty";

        private bool _isSearchResultEmptyProperty = false;

        /// <summary>
        /// Sets and gets the IsSearchResultEmpty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSearchResultEmpty
        {
            get
            {
                return _isSearchResultEmptyProperty;
            }

            set
            {
                if (_isSearchResultEmptyProperty == value)
                {
                    return;
                }

                _isSearchResultEmptyProperty = value;
                RaisePropertyChanged(IsSearchResultEmptyPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty = "";

        /// <summary>
        /// Sets and gets the SearchText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SearchText
        {
            get
            {
                return _searchTextProperty;
            }

            set
            {
                if (_searchTextProperty == value)
                {
                    return;
                }

                _searchTextProperty = value;
                RaisePropertyChanged(SearchTextPropertyName);

                doSearch();
            }
        }

        private void doSearch()
        {
            var searchContent = SearchText.Trim();
            if (string.IsNullOrEmpty(searchContent))
            {
                //��ԭ��ʼ��ʾ
                foreach (var item in PackageItems)
                {
                    item.IsSearching = false;
                }

                foreach (var item in PackageItems)
                {
                    item.SearchText = searchContent;
                }

                IsSearchResultEmpty = false;
            }
            else
            {
                //��������������ʾ

                foreach (var item in PackageItems)
                {
                    item.IsSearching = true;
                }

                //Ԥ��ȫ����Ϊ��ƥ��
                foreach (var item in PackageItems)
                {
                    item.IsMatch = false;
                }


                foreach (var item in PackageItems)
                {
                    item.ApplyCriteria(searchContent);
                }

                IsSearchResultEmpty = true;
                foreach (var item in PackageItems)
                {
                    if (item.IsMatch)
                    {
                        IsSearchResultEmpty = false;
                        break;
                    }
                }

            }
        }




        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The <see cref="IsWorkflowRunning" /> property's name.
        /// </summary>
        public const string IsWorkflowRunningPropertyName = "IsWorkflowRunning";

        private bool _isWorkflowRunningProperty = false;

        /// <summary>
        /// Sets and gets the IsWorkflowRunning property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsWorkflowRunning
        {
            get
            {
                return _isWorkflowRunningProperty;
            }

            set
            {
                if (_isWorkflowRunningProperty == value)
                {
                    return;
                }

                _isWorkflowRunningProperty = value;
                RaisePropertyChanged(IsWorkflowRunningPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowRunningToolTip" /> property's name.
        /// </summary>
        public const string WorkflowRunningToolTipPropertyName = "WorkflowRunningToolTip";

        private string _workflowRunningToolTipProperty = "";

        /// <summary>
        /// Sets and gets the WorkflowRunningToolTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WorkflowRunningToolTip
        {
            get
            {
                return _workflowRunningToolTipProperty;
            }

            set
            {
                if (_workflowRunningToolTipProperty == value)
                {
                    return;
                }

                _workflowRunningToolTipProperty = value;
                RaisePropertyChanged(WorkflowRunningToolTipPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowRunningName" /> property's name.
        /// </summary>
        public const string WorkflowRunningNamePropertyName = "WorkflowRunningName";

        private string _workflowRunningNameProperty = "";

        /// <summary>
        /// Sets and gets the WorkflowRunningName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WorkflowRunningName
        {
            get
            {
                return _workflowRunningNameProperty;
            }

            set
            {
                if (_workflowRunningNameProperty == value)
                {
                    return;
                }

                _workflowRunningNameProperty = value;
                RaisePropertyChanged(WorkflowRunningNamePropertyName);
            }
        }





        /// <summary>
        /// The <see cref="WorkflowRunningStatus" /> property's name.
        /// </summary>
        public const string WorkflowRunningStatusPropertyName = "WorkflowRunningStatus";

        private string _workflowRunningStatusProperty = "";

        /// <summary>
        /// Sets and gets the WorkflowRunningStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WorkflowRunningStatus
        {
            get
            {
                return _workflowRunningStatusProperty;
            }

            set
            {
                if (_workflowRunningStatusProperty == value)
                {
                    return;
                }

                _workflowRunningStatusProperty = value;
                RaisePropertyChanged(WorkflowRunningStatusPropertyName);
            }
        }




        private RelayCommand _stopCommand;

        /// <summary>
        /// Gets the StopCommand.
        /// </summary>
        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand
                    ?? (_stopCommand = new RelayCommand(
                    () =>
                    {
                        if(m_runManager!=null)
                        {
                            m_runManager.Stop();
                        }  
                    },
                    () => true));
            }
        }
    }
}
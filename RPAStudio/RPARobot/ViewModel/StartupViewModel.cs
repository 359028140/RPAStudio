﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using RPARobot.Librarys;
using RPARobot.Windows;
using System.Windows;
using System.Xml;
using System;

namespace RPARobot.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class StartupViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Window m_view { get; set; }

        private bool IsQuitAsking = false;

        public MainWindow MainWindow { get; set; }
        public UserPreferencesWindow UserPreferencesWindow { get; set; }
        public RegisterWindow RegisterWindow { get; set; }

        /// <summary>
        /// Initializes a new instance of the StartupViewModel class.
        /// </summary>
        public StartupViewModel()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Hide();
            }

            if(UserPreferencesWindow == null)
            {
                UserPreferencesWindow = new UserPreferencesWindow();
                UserPreferencesWindow.Hide();
            }

            if(RegisterWindow == null)
            {
                RegisterWindow = new RegisterWindow();
                RegisterWindow.Hide();
            }

            App.Current.MainWindow = MainWindow;
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

                        Init();                       
                    }));
            }
        }

        

        private void Init()
        {
            ProgramVersion = string.Format("RPARobot-{0}", Common.GetProgramVersion());

            var registerViewModel = RegisterWindow.DataContext as RegisterViewModel;
            registerViewModel.LoadRegisterInfo();

            var userPreferencesViewModel = UserPreferencesWindow.DataContext as UserPreferencesViewModel;
            userPreferencesViewModel.LoadSettings();

            if(userPreferencesViewModel.IsAutoOpenMainWindow)
            {
                //可以在此处显示MainWindow
                ShowMainWindowCommand.Execute(null);
            }

        }

        private RelayCommand _showMainWindowCommand;

        public void RefreshProgramStatus(bool isRegistered)
        {
            if (isRegistered)
            {
               ProgramStatus = "已注册";
            }
            else
            {
                ProgramStatus = "未注册";
            }
        }

        /// <summary>
        /// Gets the ShowMainWindowCommand.
        /// </summary>
        public RelayCommand ShowMainWindowCommand
        {
            get
            {
                return _showMainWindowCommand
                    ?? (_showMainWindowCommand = new RelayCommand(
                    () =>
                    {
                        MainWindow.Show();
                        MainWindow.Activate();
                    }));
            }
        }

        private RelayCommand _quitMainWindowCommand;

        /// <summary>
        /// Gets the QuitMainWindowCommand.
        /// </summary>
        public RelayCommand QuitMainWindowCommand
        {
            get
            {
                return _quitMainWindowCommand
                    ?? (_quitMainWindowCommand = new RelayCommand(
                    () =>
                    {
                        if(!IsQuitAsking)
                        {
                            IsQuitAsking = true;

                            var ret = MessageBox.Show(App.Current.MainWindow, "确定退出吗？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                            if (ret == MessageBoxResult.Yes)
                            {
                                Application.Current.Shutdown();
                            }

                            IsQuitAsking = false;
                        }

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
                        ViewModelLocator.Instance.Main.UserPreferencesCommand.Execute(null);
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
                        ViewModelLocator.Instance.Main.ViewLogsCommand.Execute(null);
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
                        ViewModelLocator.Instance.Main.RegisterProductCommand.Execute(null);
                    }));
            }
        }



        /// <summary>
        /// The <see cref="ProgramVersion" /> property's name.
        /// </summary>
        public const string ProgramVersionPropertyName = "ProgramVersion";

        private string _programVersionProperty = "";

        /// <summary>
        /// Sets and gets the ProgramVersion property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProgramVersion
        {
            get
            {
                return _programVersionProperty;
            }

            set
            {
                if (_programVersionProperty == value)
                {
                    return;
                }

                _programVersionProperty = value;
                RaisePropertyChanged(ProgramVersionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ProgramStatus" /> property's name.
        /// </summary>
        public const string ProgramStatusPropertyName = "ProgramStatus";

        private string _programStatusProperty = "";

        /// <summary>
        /// Sets and gets the ProgramStatus property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProgramStatus
        {
            get
            {
                return _programStatusProperty;
            }

            set
            {
                if (_programStatusProperty == value)
                {
                    return;
                }

                _programStatusProperty = value;
                RaisePropertyChanged(ProgramStatusPropertyName);
            }
        }








    }
}
﻿using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using WereDev.Utils.Wu10Man.Core;
using WereDev.Utils.Wu10Man.Core.Interfaces;
using WereDev.Utils.Wu10Man.Core.Interfaces.Providers;
using WereDev.Utils.Wu10Man.Core.Services;
using WereDev.Utils.Wu10Man.Helpers;
using WereDev.Utils.Wu10Man.Providers;
using WereDev.Utils.Wu10Man.Services;
using WereDev.Utils.Wu10Man.UserWindows;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Services.Store;
using StorageFolder = Windows.Storage.StorageFolder;

namespace WereDev.Utils.Wu10Man
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogWriter _logWriter = new Wu10Logger();

        public App()
            : base()
        {
            _logWriter.LogInfo("Application starting");
            try
            {
                RegisterDependencies();
                WriteStartupLogs();
                Dispatcher.UnhandledException += OnDispatcherUnhandledException;
                MainWindow = new MainWindow();
                MainWindow.Show();

                _logWriter.LogInfo("Application started");
            }
            catch (Exception ex)
            {
                _logWriter.LogError(ex);
                MessageBox.Show("An error occured attempting to initialize the application.  Check the log file for more details.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logWriter.LogInfo("Application ended");
            base.OnExit(e);
        }

        private void RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_logWriter);

            // Providers
            builder.RegisterType<ConfigurationReader>().As<IConfigurationReader>();
            builder.RegisterType<CredentialsProvider>().As<ICredentialsProvider>();
            builder.RegisterType<FileIoProvider>().As<IFileIoProvider>();
            builder.RegisterType<RegistryProvider>().As<IRegistryProvider>();
            builder.RegisterType<UserProvider>().As<IUserProvider>();
            builder.RegisterType<WindowsApiAdapter>().As<IWindowsApiProvider>();
            builder.RegisterType<WindowsServiceProviderFactory>().As<IWindowsServiceProviderFactory>();
            builder.RegisterType<WindowsPackageProvider>().As<IWindowsPackageProvider>();

            // Services
            builder.RegisterType<FileManager>().As<IFileManager>();
            builder.RegisterType<HostsFileEditor>().As<IHostsFileEditor>();
            builder.RegisterType<RegistryEditor>().As<IRegistryEditor>();
            builder.RegisterType<WindowsServiceManager>().As<IWindowsServiceManager>();
            builder.RegisterType<WindowsPackageManager>().As<IWindowsPackageManager>();

            DependencyManager.Container = builder.Build();
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logWriter.LogError(e.Exception);
            string errorMessage = string.Format("{0}\r\n\r\nCheck the logs for more details.", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void WriteStartupLogs()
        {
            var appVersion = GetType().Assembly.GetName().Version;
            _logWriter.LogInfo($"Application version: v{appVersion.ToString()}");

            var registryEditor = DependencyManager.Resolve<IRegistryEditor>();
            _logWriter.LogInfo(EnvironmentVersionHelper.GetWindowsVersion(registryEditor));
            _logWriter.LogInfo($".Net Framework: {EnvironmentVersionHelper.GetDotNetFrameworkBuild(registryEditor)}");
        }
    }
}

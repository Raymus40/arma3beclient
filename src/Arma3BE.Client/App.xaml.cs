﻿using Arma3BEClient.Common.Logging;
using System.Windows;
using System.Windows.Threading;

namespace Arma3BEClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var boostrapper = new Bootstrapper();
            boostrapper.Run();
        }

        private readonly ILog _logger = new Log();

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _logger.Info("Exit");
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception);
            e.Handled = true;
        }
    }
}
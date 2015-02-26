using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        NotificationMenu notification;
        Configuration config;
        ConfigurationWindow configwindow;
        Context context;

        private void launch()
        {
            context = new Context();
            config = new Configuration(context);
            configwindow = new ConfigurationWindow(config);
            notification = new NotificationMenu(configwindow);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var args = e.Args;

            if (args.Length > 0)
            {
                var folder = args[0];
                config = new Configuration();
                config.addSpace(folder);
                config.writeConfig();
                Application.Current.Shutdown();
            }
            else
            {
                //if (!SetRegistry.register())
                //{
                    //MessageBox.Show("launching", "");
                    launch();
                //}
                //else
                //{
                //    //MessageBox.Show("shutting down", "");
                //    Environment.Exit(0);
                //    Application.Current.Shutdown();
                //}
            }
        }
    }
}
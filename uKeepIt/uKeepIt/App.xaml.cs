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
            //bool ok;
            //var m = new System.Threading.Mutex(true, "ukeepit", out ok);
            //if (!ok)
            //{
            //    Application.Current.Shutdown();
            //}
            //GC.KeepAlive(m);

            context = new Context();
            config = new Configuration(context);
            if (config.firstStart())
            {
                new WelcomeWindow(config);
            }
            configwindow = new ConfigurationWindow(config);
            notification = new NotificationMenu(configwindow);
            context.enableGarbageCollection();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var args = e.Args;

            if (args.Length > 0)
            {  // called from the right-click menu to add a folder
                var folder = args[0];
                config = new Configuration();
                config.editor.add_space(folder);
                Application.Current.Shutdown();
            }
            else
            {  // launching the full application
                launch();
            }
        }
    }
}
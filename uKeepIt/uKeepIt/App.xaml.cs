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
        // Configuration
        //public readonly static Configuration Configuration = new Configuration();

        //private NotificationMenu notification;

        Configuration config;
        Context context;

        private void main(object sender, StartupEventArgs e)
        {
            //notification = new NotificationMenu(this);

            context = new Context();
            config = new Configuration(context);

            context.watchFolders();

            Window configwindow = new ConfigurationWindow(config);
            configwindow.Show();

            //new GarbageCollection(new ImmutableStack<Store>(config.Stores), config.MultiObjectStore.AsStack());

            //Environment.Exit(0);
        }
    }
}
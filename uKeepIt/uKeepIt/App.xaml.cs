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

        App()
        {
            context = new Context();
            config = new Configuration(context);
            configwindow = new ConfigurationWindow(config);
            notification = new NotificationMenu(configwindow);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Configuration
        public readonly static Configuration Configuration = new Configuration();

        private NotificationMenu notification;
        private FolderWatcher watcher;

        private void main(object sender, StartupEventArgs e)
        {
            notification = new NotificationMenu(this);
            watcher = new FolderWatcher();
        }
    }
}

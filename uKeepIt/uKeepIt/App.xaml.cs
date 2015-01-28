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
        //public readonly static Configuration Configuration = new Configuration();

        //private NotificationMenu notification;
        //private FolderWatcher watcher;

        private void main(object sender, StartupEventArgs e)
        {
            //notification = new NotificationMenu(this);
            //watcher = new FolderWatcher();

            ConfigurationSnapshot config = new ConfigurationSnapshot(@"C:\Users\damien\Documents\ukeepit\test\config");

            string folder = @"C:\Users\damien\Documents\ukeepit\test\local\confidential";
            SynchronizedFolder syncFolder = new SynchronizedFolder(config.Space("confidential"), folder, new SynchronizedFolderState(folder));

            byte[] keybytes = new byte[32];
            for (int i = 0; i < keybytes.Length; i++) keybytes[i] = 0x01;

            ArraySegment<byte> key = new ArraySegment<byte>(keybytes);

            new Synchronizer(syncFolder.Space.CreateEditor(key), syncFolder.Folder, config.MultiObjectStore);
        }
    }
}
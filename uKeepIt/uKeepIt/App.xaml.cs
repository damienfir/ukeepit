﻿using System;
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

        private void main(object sender, StartupEventArgs e)
        {
            //notification = new NotificationMenu(this);

            config = new Configuration();
            

            //new GarbageCollection(new ImmutableStack<Store>(config.Stores), config.MultiObjectStore.AsStack());

            //Environment.Exit(0);
        }

        private List<SynchronizedFolder> addFolders()
        {
            var folders = new List<SynchronizedFolder>();
            foreach (var space_name in config.Spaces())
            {
                string folder = @"C:\Users\damien\Documents\ukeepit\test\local\" + space_name;
                folders.Add(new SynchronizedFolder(config.Space("confidential"), folder, new SynchronizedFolderState(folder), config));
            }
            return folders;
        }

        private void syncAllFolders(List<SynchronizedFolder> folders)
        {
            foreach (var folder in folders)
            {
                folder.syncFolder();
            }
        }

    }
}
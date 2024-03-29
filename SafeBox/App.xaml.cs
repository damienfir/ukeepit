﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Threading;
using SafeBox.Burrow;

namespace SafeBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Single instance windows
        public static SingleInstanceWindow<StoresWindow> storesWindow = null;
        public static SingleInstanceWindow<LogWindow> logWindow = null;
        public static SingleInstanceWindow<WelcomeWindow> welcomeWindow= null;

        // Configuration
        public static Cards.Configuration Configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load the default configuration
            Burrow.Static.SynchronizationContext = SynchronizationContext.Current;
            Configuration = new Cards.Configuration();

            // Reload the configuration now
            //Configuration.Reload(BurrowConfiguration_Reloaded);

            // Single instance windows
            logWindow = new SingleInstanceWindow<LogWindow>(() => new LogWindow());
            storesWindow = new SingleInstanceWindow<StoresWindow>(() => new StoresWindow());
            welcomeWindow = new SingleInstanceWindow<WelcomeWindow>(() => new WelcomeWindow());

            logWindow.Show();
            storesWindow.Show();

            TraverseFolder(@"C:\Users\Thomas\Desktop\swi5t");
        }

        void BurrowConfiguration_Reloaded(Burrow.Configuration.Snapshot snapshot)
        {
            /*
            var settings = SafeBox.Properties.Settings.Default;
            var identityHash = Burrow.Hash.From(settings.Identity);
            var identity = snapshot.IdentityByHash(identityHash);
            if (identity == null) identity = snapshot.Identities.Head;

            // No identity? Then show the welcome screen
            identity = null;
            if (identity == null)
            {
                welcomeWindow.Show();
                return;
            }

            // Otherwise, show the main window
            new MainWindow(identity.Unlock(), true).Show();
             */
        }

        void TraverseFolder(string folder)
        {
            //var dataTree = new Burrow.DataTree.DataTreeConstructor();
            //var files = dataTree.Root.Label("files");

            foreach (var file in Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(file);
                if (!fileInfo.Exists) continue;
                //files.Label(file).Label("size").Add(dataTree.BigEndian.Int64(fileInfo.Length));


            }
        }
    }
}

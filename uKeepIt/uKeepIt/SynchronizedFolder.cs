using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    class SynchronizedFolder
    {
        public readonly Space Space;
        public readonly string Folder;
        public readonly SynchronizedFolderState CreatedState;
        public FileSystemWatcher watcher;
        public readonly ConfigurationSnapshot config;
        private bool changed = false;
        private bool syncing = false;
        private int n = 0;

        public SynchronizedFolder(Space space, string folder, SynchronizedFolderState createdState, ConfigurationSnapshot config)
        {
            this.Space = space;
            this.Folder = folder;
            this.CreatedState = createdState;
            this.config = config;

            this.watcher = WatchDirectory(folder);
        }

        private FileSystemWatcher WatchDirectory(string folder)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(folder);

            watcher.Changed += new FileSystemEventHandler(onChanged);
            watcher.Created += new FileSystemEventHandler(onCreated);
            watcher.Deleted += new FileSystemEventHandler(onDeleted);
            watcher.Renamed += new RenamedEventHandler(onRenamed);
            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        private void onRenamed(object sender, RenamedEventArgs e)
        {
            syncFolder();
        }

        private void onDeleted(object sender, FileSystemEventArgs e)
        {
            syncFolder();
        }

        private void onCreated(object sender, FileSystemEventArgs e)
        {
            syncFolder();
        }

        private void onChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals(".ukeepit"))
            {
                Console.WriteLine("detected a change in .ukeepit");
                return;
            }
            syncFolder();
        }

        public void syncFolder()
        {
            changed = true;
            launchSynchronizer();
        }

        private void launchSynchronizer()
        {
            // discards while running or if no changes left
            if (!changed || syncing) return;

            syncing = true;
            changed = false;

            Console.WriteLine("launching synchronizer for {0}", Folder);
            new Synchronizer(Space.CreateEditor(new ArraySegment<byte>(config.key)), Folder, config.MultiObjectStore);
            Console.WriteLine("synchronizer finished for {0}", Folder);
            syncing = false;

            // run again if a change has been made while Synchronizer was running
            launchSynchronizer();
        }
    }
}
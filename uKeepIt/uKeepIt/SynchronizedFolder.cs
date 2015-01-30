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
        private bool syncing = false;

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
            launchSynchronizer();
        }

        private void onDeleted(object sender, FileSystemEventArgs e)
        {
            launchSynchronizer();
        }

        private void onCreated(object sender, FileSystemEventArgs e)
        {
            launchSynchronizer();
        }

        private void onChanged(object sender, FileSystemEventArgs e)
        {
            launchSynchronizer();
        }

        private void launchSynchronizer()
        {
            if (!syncing)
            {
                Console.WriteLine("launching synchronizer for {0}", Folder);
                syncing = true;
                new Synchronizer(Space.CreateEditor(new ArraySegment<byte>(config.key)), Folder, config.MultiObjectStore);
                syncing = false;
            }
        }
    }
}

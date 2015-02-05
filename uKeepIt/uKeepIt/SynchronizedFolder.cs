using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class SynchronizedFolder
    {
        public readonly Context context;
        public readonly Space space;

        //public readonly SynchronizedFolderState CreatedState;
        private FileSystemWatcher _watcher;
        private bool _changed = false;
        private bool _syncing = false;

        public SynchronizedFolder(Context context, Space space)
        {
            this.context = context;
            this.space = space;

            //this.CreatedState = new SynchronizedFolderState(folder);

            SetupWatcher();
        }

        private void SetupWatcher()
        {
            _watcher = new FileSystemWatcher(space.folder);
            _watcher.Changed += new FileSystemEventHandler(onChanged);
            _watcher.Created += new FileSystemEventHandler(onCreated);
            _watcher.Deleted += new FileSystemEventHandler(onDeleted);
            _watcher.Renamed += new RenamedEventHandler(onRenamed);
        }

        public void watch()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void unwatch()
        {
            _watcher.EnableRaisingEvents = false;
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
            _changed = true;
            launchSynchronizer();
        }

        private void launchSynchronizer()
        {
            // discards while running or if no changes left
            if (!_changed || _syncing) return;

            _syncing = true;
            _changed = false;

            Console.WriteLine("launching synchronizer for {0}", space.folder);
            new Synchronizer(space.CreateEditor(context.key, context.multiobjectstore, context.stores), space.folder, context.multiobjectstore);
            Console.WriteLine("synchronizer finished for {0}", space.folder);
            _syncing = false;

            // run again if a change has been made while Synchronizer was running
            launchSynchronizer();
        }
    }
}
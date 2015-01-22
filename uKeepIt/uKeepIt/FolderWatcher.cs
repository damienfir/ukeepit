using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    class FolderWatcher
    {
        List<FileSystemWatcher> folders;

        public FolderWatcher()
        {
            folders = new List<FileSystemWatcher>();
            addFolder("C:\\Users\\damien\\Documents\\confidential");
        }

        public void addFolder(string path)
        {
            FileSystemWatcher newfolder = new FileSystemWatcher(path);
            newfolder.Changed += new FileSystemEventHandler(onChanged);
            newfolder.Created += new FileSystemEventHandler(onCreated);
            newfolder.Deleted += new FileSystemEventHandler(onDeleted);
            newfolder.Renamed += new RenamedEventHandler(onRenamed);
            
            newfolder.EnableRaisingEvents = true;

            folders.Add(newfolder);
            Console.WriteLine("folder added: {0}", path);
        }

        private void onRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("renamed {0} to {1}", e.OldFullPath, e.FullPath);
        }

        private void onDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("deleted {0}", e.FullPath);
        }

        private void onCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("created {0}", e.FullPath);
        }

        private void onChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("changed {0}", e.FullPath);
        }
    }
}
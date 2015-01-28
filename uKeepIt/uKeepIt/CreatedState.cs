using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow;

namespace uKeepIt
{
    // This contains the list of files and folders that are under our control (by checking them in or out), and their current state (size, modification date, content id).
    class SynchronizedFolderState
    {
        public readonly string Folder;
        public readonly Dictionary<string, CreatedFileEntry> FilesByPath = new Dictionary<string, CreatedFileEntry>();
        public readonly Dictionary<string, CreatedFileEntry> FilesById = new Dictionary<string, CreatedFileEntry>();
        public readonly Dictionary<Hash, CreatedFileEntry> FilesByContentId = new Dictionary<Hash, CreatedFileEntry>();
        public readonly Dictionary<string, CreatedFolderEntry> FoldersByPath = new Dictionary<string, CreatedFolderEntry>();

        public SynchronizedFolderState(string folder)
        {
            this.Folder = folder;
        }

        internal CreatedFileEntry FileEntryByPath(string name)
        {
            var entry = null as CreatedFileEntry;
            FilesByPath.TryGetValue(name, out entry);
            return entry;
        }

        internal CreatedFileEntry FileEntryById(string name, Hash contentId)
        {
            var entry = null as CreatedFileEntry;
            FilesByPath.TryGetValue(contentId.Hex() + "\0" + name, out entry);
            return entry;
        }

        internal CreatedFileEntry FileEntryByContentId(Hash contentId)
        {
            var entry = null as CreatedFileEntry;
            FilesByContentId.TryGetValue(contentId, out entry);
            return entry;
        }

        internal CreatedFolderEntry FolderEntryByPath(string path)
        {
            var entry = null as CreatedFolderEntry;
            FoldersByPath.TryGetValue(path, out entry);
            return entry;
        }

        internal void Save()
        {
            // Serialize
            string text = "";
            foreach (var file in FilesByPath)
                text += "file\t" + file.Value.ContentId.Hex() + "\t" + file.Value.Length + "\t" + file.Value.LastWriteTime.Ticks + "\t" + file.Value.Path + "\n";
            foreach (var folder in FoldersByPath)
                text += "folder\t" + folder.Value.Path + "\n";

            // Write the new file
            var newFile = Folder + "\\state-" + DateTime.UtcNow.Ticks;
            var stream = System.IO.File.OpenWrite(newFile);
            var bytes = text.ToByteArray();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }

        internal void Load()
        {
            // Find the newest file
            var newest = 0L;
            var newestFile = null as string;
            foreach (var file in MiniBurrow.Static.DirectoryEnumerateFiles(Folder))
            {
                if (!file.StartsWith("state-")) continue;
                var ticks = 0L;
                if (!long.TryParse(file.Substring(6), out ticks)) continue;
                if (ticks < newest) continue;
                newest = ticks;
                newestFile = file;
            }

            if (newestFile == null) return;

            // Read
            var lines = MiniBurrow.Static.FileUtf8Lines(Folder + "\\" + newestFile, null);
            if (lines == null) return;
            foreach (var line in lines)
            {
                var args = line.Split('\t');
                if (args == null || args.Length < 2) continue;
                if (args[0] == "file")
                {
                    if (args.Length < 5) continue;
                    var contentId = Hash.From(args[1]);
                    if (contentId == null) continue;
                    var size = 0L;
                    if (!long.TryParse(args[2], out size)) continue;
                    var ticks = 0L;
                    if (!long.TryParse(args[3], out ticks)) continue;
                    Add(new CreatedFileEntry(args[4], new DateTime(ticks), size, contentId));
                }
                else if (args[0] == "folder")
                {
                    Add(new CreatedFolderEntry(args[1]));
                }
            }
        }

        public void Add(CreatedFileEntry entry)
        {
            FilesByPath.Add(entry.Path, entry);
            FilesById.Add(entry.ContentId.Hex() + "\0" + entry.Path, entry);
        }
 
        public void Add(CreatedFolderEntry entry)
        {
            FoldersByPath.Add(entry.Path, entry);
        }



    }
}

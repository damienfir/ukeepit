﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.Folder
{
    public class ObjectStore : Backend.ObjectStore
    {
        public static ObjectStore ForUrl(string url)
        {
            var folder = Static.ToAbsolutePath(Static.UrlToWindowsFolder(url));
            if (folder == null) return null;
            return new ObjectStore(url, Path.GetFullPath(folder));
        }
        
        public readonly string Folder;

        public ObjectStore(string url, string folder)
            : base(url, 10)
        {
            this.Folder = folder;
        }

        bool Has(Hash hash)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            return File.Exists(file);
        }

        BurrowObject Get(Hash hash)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            return BurrowObject.From(Burrow.Static.FileBytes(file, null));
        }

        Hash Put(BurrowObject obj, PrivateIdentity identity)
        {
            // Check if that object exists already
            var hash = obj.Hash();
            var hashHex = hash.Hex();
            var folder = Folder + "\\" + hashHex.Substring(0, 2);
            var file = folder + "\\" + hashHex.Substring(2);
            if (File.Exists(file)) return hash; 

            // Write the file, and move it to the right place
            if (!Directory.Exists(folder)) Burrow.Static.DirectoryCreate(folder, LogLevel.Warning);
            var temporaryFile = folder + "\\." + Burrow.Static.RandomHex(16);
            if (Burrow.Static.WriteFile(temporaryFile, obj.Bytes) && Burrow.Static.FileMove(temporaryFile, file, LogLevel.Warning) && File.Exists(file)) return hash;
            return null;
        }
    }
}

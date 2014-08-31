using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Folder
{
    public class ObjectStore : Abstract.ObjectStore
    {
        public static Abstract.ObjectStore ForUrl(string url)
        {
            var folder = Static.ToAbsolutePath(Static.UrlToWindowsFolder(url));
            if (folder == null) return null;
            return new ObjectStore(url, Path.GetFullPath(folder));
        }
        
        public readonly string Folder;

        public ObjectStore(string url, string folder)
            : base(url)
        {
            this.Folder = folder;
        }

        public override void HasObject(Hash hash, HasObjectResult handler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(dummy => HasObjectAsync(hash, handler)));
        }

        void HasObjectAsync(Hash hash, HasObjectResult handler)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            var result = File.Exists(file);
            Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(result)), null);
        }

        public override void GetObject(Hash hash, GetObjectResult handler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(dummy => GetObjectAsync(hash, handler)));
        }

        void GetObjectAsync(Hash hash, GetObjectResult handler)
        {
            var hashHex = hash.Hex();
            var file = Folder + "\\" + hashHex.Substring(0, 2) + "\\" + hashHex.Substring(2);
            handler(BurrowObject.From(Burrow.Static.ReadFile(file, null)));
        }

        public override void PutObject(BurrowObject obj, UnlockedPrivateIdentity identity, PutObjectResult handler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(dummy => PutObjectAsync(obj, identity, handler)));
        }

        void PutObjectAsync(BurrowObject obj, UnlockedPrivateIdentity identity, PutObjectResult handler)
        {
            // Check if that object exists already
            var hash = obj.Hash();
            var hashHex = hash.Hex();
            var folder = Folder + "\\" + hashHex.Substring(0, 2);
            var file = folder + "\\" + hashHex.Substring(2);
            if (File.Exists(file)) { handler(hash); return; }

            // Write the file, and move it to the right place
            if (!Directory.Exists(folder)) Burrow.Static.DirectoryCreate(folder);
            var temporaryFile = folder + "\\." + Burrow.Static.RandomHex(16);
            if (Burrow.Static.WriteFile(temporaryFile, obj.Bytes) && Burrow.Static.FileMove(temporaryFile, file) && File.Exists(file)) { handler(hash); return; }
            handler(null);
        }
    }
}

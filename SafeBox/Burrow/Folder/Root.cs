using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace SafeBox.Burrow.Folder
{
    public class Root : Abstract.Root
    {
        public readonly string Folder;

        public Root(Account account, string name)
            : base(account, name)
        {
            this.Folder= account.Folder + "\\" + name;
        }

        public override void List(ListResult handler) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj => ListAsync(handler)));
        }
        
        void ListAsync(ListResult handler)
        {
            var list = new List<ObjectUrl>();
            var identityHashHex = Account.IdentityHash.Hex();

            foreach (var file in Burrow.Static.DirectoryEnumerateFilesSilent(Folder))
            {
                var name = Path.GetFileName(file);
                if (name.Length < 64) continue;
                var hash = Hash.From(name.Substring(0, 64));
                if (hash == null) continue;
                var url = Burrow.Static.ReadFileSilent(file, null as string);
                if (url == null) continue;
                list.Add(new ObjectUrl(url, hash));
            }

            Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(list)), null);
        }

        public override void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, UnlockedPrivateIdentity identity, PostResult handler) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj => PostAsync( add, remove, identity, handler)));
        }

        void PostAsync(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, UnlockedPrivateIdentity identity, PostResult handler)
        {
            // List of files to remove
            var hexHashesToRemove = new SortedSet<string>();
            foreach (var hash in remove) hexHashesToRemove.Add(hash.Hex());

            // Create the folder to add hashes if necessary
            var identityHashHex = Account.IdentityHash.Hex();
            Burrow.Static.DirectoryCreate(Folder);
            // TODO: set access rights 
            //mkdir $o->{account}->{folder}, 0755;
            //mkdir $o->{folder},	$o->{name} eq 'identity' ? 0755 :
            //					    $o->{name} eq 'in-queue' ? 0733 : 0700;

            // Add
            var sha256 = new System.Security.Cryptography.SHA256Managed();
            foreach (var objectUrl in add)
            {
                var url = objectUrl.Url;
                var fileContent = url == null ? new ArraySegment<byte>(new byte[0]) : new ArraySegment<byte>(Encoding.UTF8.GetBytes(url));
                var temporaryFile = Folder + "\\." + Burrow.Static.RandomHex(16);
                if (!Burrow.Static.WriteFile(temporaryFile, fileContent)) { Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(false)), null); return; }
                var suffix = url == null ? "" : "-" + Burrow.Static.BytesToHexString(sha256.ComputeHash(fileContent.Array), 0, 16);
                var hashHex = objectUrl.Hash.Hex();
                var file = Folder + '\\' + hashHex + suffix;
                Burrow.Static.FileMoveSilent(temporaryFile, file);
                Burrow.Static.FileDeleteSilent(temporaryFile);
                if (!File.Exists(file)) { Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(false)), null); return; }
                hexHashesToRemove.Remove(hashHex);
            }

            // Remove (and ignore all errors)
            if (hexHashesToRemove.Count > 0)
                foreach (var file in Burrow.Static.DirectoryEnumerateFilesSilent(Folder))
                {
                    var name = Path.GetFileName(file);
                    if (name.Length < 64) continue;
                    if (hexHashesToRemove.Contains(name.Substring(0, 64))) Burrow.Static.FileDelete(file);
                }

            Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(true)), null);
        }
    }
}

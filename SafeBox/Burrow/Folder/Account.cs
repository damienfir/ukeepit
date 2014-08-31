using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SafeBox.Burrow.Folder
{
    public class Account : Abstract.Account
    {
        public readonly string Folder;

        public Account(AccountStore store, Hash identityHash)
            : base(store, identityHash)
        {
            Folder = store.Folder + "\\" + identityHash.Hex();
        }

        public override Abstract.Root Root(string name)
        {
            if (!IsValidRootName(name)) return null;
            return new Root(this, name);
        }

        public override void Delete(DeleteResult handler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj => DeleteAsync(handler)));   
        }

        void DeleteAsync(DeleteResult handler) 
        {
            var suffix = "-deleted-" + Burrow.Static.RandomHex(8);
            var success = Burrow.Static.DirectoryMove(Folder, Folder + suffix);
            Burrow.Static.DirectoryDelete(Folder + suffix);
            Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(success)), null);
        }

        // Safe root names
        public static bool IsValidRootName(string name)
        {
            if (name.Length < 1) return false;

            var f = name[0];
            if (f != '_' && f != '#' && f != '=' && f != '@' && f != '+' && f != '-' && (f < 'a' || f > 'z') && (f < '0' || f > '9'))
                return false;

            foreach (var c in name)
                if (c != '_' && c != '#' && c != '=' && c != '@' && c != '+' && c != '-' && c != '.' && c != '!' && (c < 'a' || c > 'z') && (c < '0' || c > '9'))
                    return false;

            return true;
        }
    }
}

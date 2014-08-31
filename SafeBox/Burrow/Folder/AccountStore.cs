using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace SafeBox.Burrow.Folder
{
    public class AccountStore: Abstract.AccountStore
    {
        public static Abstract.AccountStore ForUrlAsync(string url)
        {
            var folder = Static.ToAbsolutePath(Static.UrlToWindowsFolder(url));
            if (folder == null) return null;
            return new AccountStore(url, Path.GetFullPath(folder));
        }

        public readonly string Folder;

        public AccountStore(string url, string folder)
            : base(url)
        {
            this.Folder = folder;
        }

        public override void Accounts(Dictionary<string, string> query, AccountsResult handler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj => AccountsAsync(query, handler)));
        }

        void AccountsAsync(Dictionary<string, string> query, AccountsResult handler)
        {
            var accounts = new Dictionary<string, Hash>();
            foreach (var file in Burrow.Static.DirectoryEnumerateDirectories(Folder))
            {
                var name = Path.GetFileName(file);
                var hash = Hash.From(name);
                if (hash == null) continue;
                accounts[name] = hash;
            }

            var accountList = accounts.Select(tuple => new Account(this, tuple.Value));
            Burrow.Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(accountList)), null);
        }

        public override Abstract.Account Account(Hash identityHash) { return new Account(this, identityHash); }

        //public string AccountFolder(FolderAccount account) { return BaseFolder + "\\" + currentVersion + "\\" + account.IdentityHash.Hex(); }
        //public string RootFolder(FolderRoot root) { return AccountFolder(root.FolderAccount) + "\\" + root.Name; }
        public override void Accounts(Dictionary<string, string> query, AccountsResult handler) { handler(new List<Abstract.Account>()); }
        public override Abstract.Account Account(Hash identityHash) { return null; }
    }
}

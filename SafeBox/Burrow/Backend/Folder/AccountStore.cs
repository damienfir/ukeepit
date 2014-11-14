using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.Folder
{
    public class AccountStore : Backend.AccountStore
    {
        public static AccountStore ForUrl(string url)
        {
            var folder = Static.ToAbsolutePath(Static.UrlToWindowsFolder(url));
            if (folder == null) return null;
            return new AccountStore(url, Path.GetFullPath(folder));
        }

        public readonly string Folder;

        public AccountStore(string url, string folder)
            : base(url, 10)
        {
            this.Folder = folder;
        }

        public IEnumerable<Hash> Accounts(Dictionary<string, string> query)
        {
            var accounts = new ImmutableStack<Hash>();
            foreach (var file in Burrow.Static.DirectoryEnumerateDirectories(Folder))
            {
                var name = Path.GetFileName(file);
                var hash = Hash.From(name);
                if (hash == null) continue;
                accounts = accounts.With(hash);
            }

            return accounts;
        }

        public IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel)
        {
            if (!IsValidRootLabel(rootLabel)) return null;
            var rootFolder = Folder + "\\" + identityHash.Hex() + "\\" + rootLabel;

            var list = new ImmutableStack<ObjectUrl>();
            foreach (var file in Burrow.Static.DirectoryEnumerateFiles(rootFolder))
            {
                var name = Path.GetFileName(file);
                if (name.Length < 64) continue;
                var hash = Hash.From(name.Substring(0, 64));
                if (hash == null) continue;
                var url = Burrow.Static.FileUtf8Text(file, null as string);
                list = list.With(new ObjectUrl(url, hash));
            }

            return list;
        }

        public bool Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity)
        {
            if (!IsValidRootLabel(rootLabel)) return false;
            var rootFolder = Folder + "\\" + identityHash.Hex() + "\\" + rootLabel;

            // List of files to remove
            var hexHashesToRemove = new SortedSet<string>();
            foreach (var hash in remove) hexHashesToRemove.Add(hash.Hex());

            // Create the folder to add hashes if necessary
            Burrow.Static.DirectoryCreate(rootFolder, LogLevel.Warning);
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
                var temporaryFile = rootFolder + "\\." + Burrow.Static.RandomHex(16);
                if (!Burrow.Static.WriteFile(temporaryFile, fileContent)) return false;
                var suffix = url == null ? "" : "-" + Burrow.Serialization.Text.ToHexString(sha256.ComputeHash(fileContent.Array), 0, 16);
                var hashHex = objectUrl.Hash.Hex();
                var file = rootFolder + '\\' + hashHex + suffix;
                Burrow.Static.FileMove(temporaryFile, file, LogLevel.Warning);
                Burrow.Static.FileDelete(temporaryFile, LogLevel.Warning);
                if (!File.Exists(file)) return false;
                hexHashesToRemove.Remove(hashHex);
            }

            // Remove (and ignore all errors)
            if (hexHashesToRemove.Count > 0)
                foreach (var file in Burrow.Static.DirectoryEnumerateFiles(rootFolder))
                {
                    var name = Path.GetFileName(file);
                    if (name.Length < 64) continue;
                    if (hexHashesToRemove.Contains(name.Substring(0, 64))) Burrow.Static.FileDelete(file, LogLevel.Warning);
                }

            return true;
        }

        bool Delete(Hash identityHash)
        {
            var accountFolder = Folder + "\\" + identityHash.Hex();
            var suffix = "-deleted-" + Burrow.Static.RandomHex(8);
            var success = Burrow.Static.DirectoryMove(accountFolder, accountFolder + suffix, LogLevel.Warning);
            Burrow.Static.DirectoryDelete(accountFolder + suffix, LogLevel.Warning);
            return success;
        }

        // Safe root names
        public static bool IsValidRootLabel(string name)
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

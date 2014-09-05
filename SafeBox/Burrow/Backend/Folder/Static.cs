using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SafeBox.Burrow.Backend.Folder
{
    public class Static
    {
        static Regex localDriveUrl = new Regex(@"^[A-Za-z]:\\");
        static Regex posixLocalDriveUrl = new Regex(@"^\/([A-Za-z]):?(\/.*)$");
        static Regex fileServerUrl = new Regex(@"^file:\/\/[^\/]*(\/.*)$");
        static Regex fileUrl = new Regex(@"^file:\/*(\/.*)$");
        static Regex fileRelativeUrl = new Regex(@"^file:(.*)$");

        public static string UrlToWindowsFolder(string url)
        {
            if (url.StartsWith("\\\\")) return url;
            if (url.StartsWith("//")) return url.Replace('/', '\\');

            var match = localDriveUrl.Match(url);
            if (match.Success) return url;

            match = posixLocalDriveUrl.Match(url);
            if (match.Success) return match.Groups[1].Value + ":" + match.Groups[2].Value.Replace('/', '\\');

            match = fileServerUrl.Match(url);
            if (match.Success) return match.Groups[1].Value.Replace('/', '\\');

            match = fileUrl.Match(url);
            if (match.Success) return match.Groups[1].Value.Replace('/', '\\');

            match = fileRelativeUrl.Match(url);
            if (match.Success) return match.Groups[1].Value.Replace('/', '\\');

            return null;
        }

        public static string ToAbsolutePath(string folder)
        {
            if (folder == null) return null;
            try { return Path.GetFullPath(folder); }
            catch (Exception) { return null; }
        }
    }
}

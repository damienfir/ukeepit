using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SafeBox.Burrow
{
    public class ObjectUrl
    {
        public Hash Hash;
        public string Url;

        public ObjectUrl(string url, Hash hash)
        {
            this.Hash = hash;
            this.Url = url;
        }

        public ObjectUrl(Hash hash)
        {
            this.Hash = hash;
        }

        public string ToText() { return (Url == null ? "" : Url + "/") + Hash.Hex(); }

        // Static methods
        private static Regex objectUrlInTextRegex = new Regex(@"^\s*(.*?)[\/\\]+([0-9a-fA-F\/\\]*)$");

        public static ObjectUrl FromText(string text)
        {
            var match = objectUrlInTextRegex.Match(text);
            if (match.Success)
            {
                var hashHex = match.Groups[2].Value.Replace("\\", "").Replace("/", "");
                if (hashHex.Length == 64) return new ObjectUrl(match.Groups[1].Value, new Hash(hashHex, null));
            }
            var hash = Hash.From(text);
            return hash == null ? null : new ObjectUrl(null, hash);
        }
    }
}

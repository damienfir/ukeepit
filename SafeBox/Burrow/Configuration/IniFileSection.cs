using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Configuration
{
    public class IniFileSection
    {

        // *** Object ***

        public readonly Dictionary<string, string> Pairs = new Dictionary<string, string>();

        public IniFileSection() { }

        public IniFileSection Clone()
        {
            var section = new IniFileSection();
            foreach (var tuple in Pairs) section.Pairs[tuple.Key] = tuple.Value;
            return section;
        }

        public string ToText()
        {
            var text = "";
            foreach (var tuple in Pairs)
            {
                var value = tuple.Value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\0", "\\0");
                text += tuple.Key + " = " + value + "\n";
            }
            return text;
        }

        public static bool ValidKey(string key)
        {
            if (key.StartsWith(";")) return false;
            if (key.Contains('\r')) return false;
            if (key.Contains('\n')) return false;
            if (key.Contains('\0')) return false;
            if (key.Contains('=')) return false;
            return true;
        }

        // Text tuples

        public string Get(string key, string defaultValue = null)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            return value;
        }

        public void Remove(string key)
        {
            Pairs.Remove(key);
        }

        public void Set(string key, string value)
        {
            if (!ValidKey(key)) return;
            value.Replace('\0', ' ');
            value.Replace('\n', ' ');
            value.Replace('\r', ' ');
            Pairs[key] = value.Trim();
        }

        public void Set(string key, DateTime utcDate)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = utcDate.ToUtcString();
        }

        public void Set(string key, bool value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value ? "yes" : "no";
        }

        public void Set(string key, int value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value.ToString("0");
        }

        public void Set(string key, long value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value.ToString("0");
        }

        public void Set(string key, double value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value.ToString();
        }

        public void Set(string key, byte[] value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value.ToHexString();
        }

        public void Set(string key, ArraySegment<byte> value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = value.ToHexString();
        }

        public void Set(string key, Hash hash)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = hash.Hex();
        }
    }
}

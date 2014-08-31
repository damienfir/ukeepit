using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Configuration
{
    public class Dictionary
    {
        // *** Static ***

        public static Dictionary EmptyDictionary = new Dictionary();

        public static Dictionary From(ArraySegment<byte> bytes)
        {
            if (bytes.Array == null) return null;
            return From(Static.Utf8BytesToText(bytes.Array, bytes.Offset, bytes.Count));
        }

        public static Dictionary From(byte[] bytes)
        {
            if (bytes == null) return null;
            return From(Static.Utf8BytesToText(bytes));
        }

        public static Dictionary From(string text)
        {
            if (text == null) return null;
            var lines = text.Split('\n');
            var dictionary = new Dictionary();
            foreach (var line in lines)
            {
                var pos = line.IndexOf(':');
                if (pos < 0) continue;
                var key = line.Substring(0, pos).Trim();
                dictionary.Set(key, line.Substring(pos + 1).Trim());
            }
            return dictionary;
        }

        // *** Object ***

        public readonly Dictionary<string, string> Pairs = new Dictionary<string, string>();

        public Dictionary() { }

        public Dictionary Clone()
        {
            var dictionary = new Dictionary();
            foreach (var tuple in Pairs) dictionary.Set(tuple.Key, tuple.Value);
            return dictionary;
        }

        private string ToText()
        {
            var text = "";
            foreach (var tuple in Pairs)
                text += tuple.Key + ": " + tuple.Value + "\n";
            return text;
        }

        public byte[] ToBytes() { return Static.TextToUtf8Bytes(ToText()); }

        public static bool ValidKey(string key)
        {
            if (key.StartsWith("#")) return false;
            if (key.Contains('\r')) return false;
            if (key.Contains('\n')) return false;
            if (key.Contains('\0')) return false;
            if (key.Contains('=')) return false;
            if (key.Contains(':')) return false;
            return true;
        }

        // Text tuples

        public string Get(string key, string defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            return value;
        }

        public DateTime Get(string key, DateTime defaultUtcDate)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultUtcDate;
            return Static.TextToUtcDate(value, defaultUtcDate);
        }

        public bool Get(string key, bool defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            value = value.ToLower();
            if (value == "true") return true;
            if (value == "yes") return true;
            if (value == "y") return true;
            if (value == "false") return false;
            if (value == "no") return false;
            if (value == "n") return false;
            double doubleValue;
            if (!double.TryParse(value, out doubleValue)) return false;
            return doubleValue != 0;
        }

        public int Get(string key, int defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            int.TryParse(value, out defaultValue);
            return defaultValue;
        }

        public long Get(string key, long defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            long.TryParse(value, out defaultValue);
            return defaultValue;
        }

        public double Get(string key, double defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            double.TryParse(value, out defaultValue);
            return defaultValue;
        }

        public byte[] Get(string key, byte[] defaultValue)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return defaultValue;
            return Static.HexStringToBytes(value);
        }

        public Hash GetHash(string key)
        {
            string value;
            if (!Pairs.TryGetValue(key, out value)) return null;
            return Hash.FromText(value);
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
            Pairs[key] = Static.UtcDateToText(utcDate);
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
            Pairs[key] = Static.BytesToHexString(value);
        }

        public void Set(string key, ArraySegment<byte> value)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = Static.BytesToHexString(value.Array, value.Offset, value.Count);
        }

        public void Set(string key, Hash hash)
        {
            if (!ValidKey(key)) return;
            Pairs[key] = hash.Hex();
        }
    }
}

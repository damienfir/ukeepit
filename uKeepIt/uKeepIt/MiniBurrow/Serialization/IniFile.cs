using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace uKeepIt.MiniBurrow.Serialization
{
    public class IniFile
    {
        // *** Static ***

        public static IniFile From(ArraySegment<byte> bytes)
        {
            if (bytes.Array == null) return null;
            return From(Static.Utf8BytesToText(bytes.Array, bytes.Offset, bytes.Count));
        }

        public static IniFile From(byte[] bytes)
        {
            if (bytes == null) return null;
            return From(Static.Utf8BytesToText(bytes));
        }

        private static Regex comment = new Regex(@"^\s*(.*?)\s+[;#]");
        private static Regex unescape = new Regex(@"\\(.)");

        public static IniFile From(string text)
        {
            if (text == null) return null;
            return From(Regex.Split(text, "[\n\r]+"));
        }

        public static IniFile From(string[] lines)
        {
            var iniFile = new IniFile();
            var section = iniFile.Section("");
            foreach (var line in lines)
            {
                // Comment
                var commentMatch = comment.Match(line);
                var trimmedLine = commentMatch.Success ? commentMatch.Groups[1].Value : line.Trim();

                // Section
                if (trimmedLine.Length>2 && trimmedLine[0] == '[' && trimmedLine[trimmedLine.Length - 1] == ']')
                {
                    section = iniFile.Section(trimmedLine.Substring(1, trimmedLine.Length - 2).Trim());
                    continue;
                }

                // Value
                var pos = line.IndexOf('=');
                if (pos < 0) continue;
                var key = line.Substring(0, pos).Trim();
                var value = line.Substring(pos + 1).Trim();
                if (value.Length == 0) continue;
                if (value[0] == '"' && value[value.Length - 1] == '"' && value.Length >= 2) value = value.Substring(1, value.Length - 2);
                section.Set(key, unescape.Replace(value, UnescapeChar));
            }
            return iniFile;
        }

        private static string UnescapeChar(Match m)
        {
            var c = m.Groups[1].Value;
            if (c == "0") return "\0";
            if (c == "a") return "\a";
            if (c == "b") return "\b";
            if (c == "n") return "\n";
            if (c == "r") return "\r";
            if (c == "t") return "\t";
            return c;
        }

        // *** Object ***

        public readonly Dictionary<string, IniFileSection> SectionsByName = new Dictionary<string, IniFileSection>();

        public IniFile() { }

        public ImmutableDictionary<string, string> SectionAsImmutableDictionary(string name)
        {
            var section = null as IniFileSection;
            if (SectionsByName.TryGetValue(name, out section)) return ImmutableDictionary.From(section.Pairs);
            return new ImmutableDictionary<string, string>();
        }

        public IniFileSection Section(string name)
        {
            var section = null as IniFileSection;
            if (SectionsByName.TryGetValue(name, out section)) return section;
            section = new IniFileSection();
            SectionsByName[name] = section;
            return section;
        }

        public string ToText()
        {
            var section = null as IniFileSection;
            var text = "";
            if (SectionsByName.TryGetValue("", out section) && section.Pairs.Count>0) text += section.ToText();
            foreach (var pair in SectionsByName)
                if (pair.Key != "") text += "[" + pair.Key + "]\r\n" + pair.Value.ToText();
            return text;
        }

        public byte[] ToBytes() { return Static.TextToUtf8Bytes(ToText()); }
    }
}

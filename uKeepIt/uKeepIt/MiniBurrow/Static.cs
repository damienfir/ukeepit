using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using uKeepIt.MiniBurrow.Serialization;
using uKeepIt.MiniBurrow.Aes;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt.MiniBurrow
{
    public static class Static
    {

        // For simplicity, the following variables are allocated statically (global variables).
        // While an application may work with multiple configurations (unlikely, but thinkable), it makes sense that they all share these objects.
        public static System.Threading.SynchronizationContext SynchronizationContext = new System.Threading.SynchronizationContext();
        public static readonly Log Log = new Log();
        public static Random Random = new Random();

        public static string RandomHex(int length)
        {
            var hexChars = "0123456789abcdef";
            var chars = new char[length];
            for (var i = 0; i < length; i++) chars[i] = hexChars[Random.Next(16)];
            return new string(chars);
        }

        public static byte[] RandomByteArray(int length){
            var bytes = new byte[length];
            Random.NextBytes(bytes);
            return bytes;
        }

        public static ArraySegment<byte> RandomBytes(int length)
        {
            var bytes = new byte[length];
            Random.NextBytes(bytes);
            return bytes.ToByteSegment();
        }    

        // Size formatting
        static Regex NumberWithUnit = new Regex(@"^\s*([0-9\.]+)\s*([A-Za-z]*)");

        public static long ParseByteSize(string text)
        {
            var match = NumberWithUnit.Match(text);
            if (!match.Success) return -1;
            var value = 0.0;
            Double.TryParse(match.Groups[1].Value, out value);
            var unit = match.Groups[2].Value.ToLower();
            if (unit == "kib") return (long)(value * 1024);
            if (unit == "mib") return (long)(value * 1024 * 1024);
            if (unit == "gib") return (long)(value * 1024 * 1024 * 1024);
            if (unit == "tib") return (long)(value * 1024 * 1024 * 1024 * 1024);
            if (unit == "kb") return (long)(value * 1000);
            if (unit == "mb") return (long)(value * 1000 * 1000);
            if (unit == "gb") return (long)(value * 1000 * 1000 * 1000);
            if (unit == "tb") return (long)(value * 1000 * 1000 * 1000 * 1000);
            return (long)value;
        }

        // *** File operations ***

        public static byte[] FileBytes(string file, byte[] defaultBytes = null, int logLevel = LogLevel.Debug)
        {
            try { return File.ReadAllBytes(file); }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to read file '" + file + "'. " + ex.ToString());
                return defaultBytes;
            }
        }

        public static string FileUtf8Text(string file, string defaultText, int logLevel = LogLevel.None)
        {
            try { return System.IO.File.ReadAllText(file, Encoding.UTF8); }
            catch (Exception ex) {
                Static.Log.Message(logLevel, "Failed to read file '" + file + "'. " + ex.ToString());
                return defaultText; 
            }
        }

        public static string[] FileUtf8Lines(string file, string[] defaultLines, int logLevel = LogLevel.None)
        {
            try
            {
                var content = System.IO.File.ReadAllText(file, Encoding.UTF8);
                return Regex.Split(content, "[\n\r]+");
            }
            catch (Exception) { return new string[0]; }
        }

        public static bool WriteFile(string file, ArraySegment<byte> bytes) { return WriteFile(file, bytes.Array, bytes.Offset, bytes.Count); }
        public static bool WriteFile(string file, byte[] bytes) { return WriteFile(file, bytes, 0, bytes.Length); }
        public static bool WriteFile(string file, byte[] bytes, int offset, int count)
        {
            FileStream stream = null;
            try
            {
                stream = File.Create(file);
                stream.Write(bytes, offset, count);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Error("Failed to write file '" + file + "'. " + ex.ToString());
                return false;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
        }

        public static bool FileMove(string source, string destination, int logLevel = LogLevel.None)
        {
            try
            {
                File.Move(source, destination);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to move file '" + source + "' to '" + destination + "'. " + ex.ToString());
                return false;
            }
        }

        public static bool FileDelete(string file, int logLevel = LogLevel.None)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to delete file '" + file + "'." + ex.ToString());
                return false;
            }
        }

        // *** Directory operations ***

        public static bool DirectoryMove(string source, string destination, int logLevel = LogLevel.None)
        {
            try
            {
                Directory.Move(source, destination);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to move '" + source + "' to '" + destination + "'. " + ex.ToString());
                return false;
            }
        }

        public static bool DirectoryDelete(string directory, int logLevel = LogLevel.None)
        {
            try
            {
                Directory.Delete(directory);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to remove '" + directory + "'. " + ex.ToString());
                return false;
            }
        }

        public static bool DirectoryCreate(string folder, int logLevel = LogLevel.None)
        {
            try
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to create folder '" + folder + "'. " + ex.ToString());
                return false;
            }
        }

        public static IEnumerable<string> DirectoryEnumerateFiles(string folder, int logLevel = LogLevel.None)
        {
            try { return Directory.EnumerateFiles(folder); }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to enumerate files in folder '" + folder + "'. " + ex.ToString());
                return new string[0];
            }
        }

        public static IEnumerable<string> DirectoryEnumerateDirectories(string folder, int logLevel = LogLevel.None)
        {
            try { return Directory.EnumerateDirectories(folder); }
            catch (Exception ex)
            {
                Static.Log.Message(logLevel, "Failed to enumerate directories in folder '" + folder + "'. " + ex.ToString());
                return new string[0];
            }
        }
        
        public static string Utf8BytesToText(ArraySegment<byte> bytes)
        {
            try { return Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count); }
            catch (Exception) { return null; }
        }

        public static string Utf8BytesToText(byte[] bytes, int offset, int count)
        {
            try { return Encoding.UTF8.GetString(bytes, offset, count); }
            catch (Exception) { return null; }
        }

        public static string Utf8BytesToText(byte[] bytes)
        {
            try { return Encoding.UTF8.GetString(bytes); }
            catch (Exception) { return null; }
        }

        public static byte[] TextToUtf8Bytes(string text)
        {
            try { return Encoding.UTF8.GetBytes(text); }
            catch (Exception) { return null; }
        }

        /*public static string UtcDateToText(DateTime utcDate) { return utcDate.ToString("yyyyMMddTHHmmssZ"); }

        public static DateTime TextToUtcDate(string text, DateTime defaultUtcDate)
        {
            text = text.Trim();
            if (text.Length != 16) return defaultUtcDate;
            if (text[8] != 'T') return defaultUtcDate;
            if (text[15] != 'Z') return defaultUtcDate;

            var year = DatePart(text, 0, 4, 1, 9999);
            if (year < 0) return defaultUtcDate;

            var month = DatePart(text, 4, 2, 1, 12);
            if (month < 0) return defaultUtcDate;

            var day = DatePart(text, 6, 2, 1, 31);
            if (day < 0) return defaultUtcDate;

            var hour = DatePart(text, 9, 2, 0, 60);
            if (hour < 0) return defaultUtcDate;

            var minute = DatePart(text, 11, 2, 0, 60);
            if (minute < 0) return defaultUtcDate;

            var second = DatePart(text, 13, 2, 0, 60);
            if (second < 0) return defaultUtcDate;

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }

        private static int DatePart(string text, int offset, int length, int min, int max)
        {
            int v = 0;
            for (int i = 0; i < length; i++)
            {
                var c = text[offset + i];
                if (c < '0' || c > '9') return -1;
                v = v * 10 + (c - '0');
            }
            if (v < min) return -1;
            if (v > max) return -1;
            return v;
        } */

        // DateTime conversion
        public static long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
        public static long Timestamp(DateTime d) { return (d.Ticks - UnixEpochTicks ) / 10000; }
        public static DateTime Timestamp(long t) { return new DateTime((t + UnixEpochTicks) * 10000); }

        // Compare byte array segments
        public static int Compare(ArraySegment<byte> segment0, ArraySegment<byte> segment1)
        {
            var minCount = segment0.Count < segment1.Count ? segment0.Count : segment1.Count;
            for (var n = 0; n < minCount; n++)
            {
                if (segment0.Array[segment0.Offset + n] > segment1.Array[segment1.Offset + n]) return 1;    // abd > abc
                if (segment0.Array[segment0.Offset + n] < segment1.Array[segment1.Offset + n]) return -1;   // abc < abd
            }
            if (segment0.Count > segment1.Count) return 1;    // abcd > ab
            if (segment0.Count < segment1.Count) return -1;   // ab < abcd
            return 0;
        }

        public static bool IsEqual(ArraySegment<byte> segment0, ArraySegment<byte> segment1)
        {
            if (segment0.Count != segment1.Count) return false;
            for (var n = 0; n < segment0.Count; n++)
                if (segment0.Array[segment0.Offset + n] != segment1.Array[segment1.Offset + n]) return false;
            return true;
        }

        public static byte[] ToByteArray(this ArraySegment<byte> segment) {
            if (segment.Array == null) return null;
            if (segment.Offset==0 && segment.Count == segment.Array.Length) return segment.Array;
            var bytes = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, bytes, 0, segment.Count);
            return bytes;
        }

        public static ArraySegment<byte> ToByteSegment(this byte[] byteArray) { return byteArray == null ? default(ArraySegment<byte>) : new ArraySegment<byte>(byteArray); }

        // Empty byte sequences
        public static readonly byte[] EmptyByteArray = new byte[0];
        public static byte[] NullByteArray = {0x00};
        public static readonly ArraySegment<byte> EmptyByteSegment = new ArraySegment<byte>(EmptyByteArray, 0, 0);
        public static readonly ArraySegment<byte> NullByteSegment = new ArraySegment<byte>(NullByteArray, 0, 0);
    }
}

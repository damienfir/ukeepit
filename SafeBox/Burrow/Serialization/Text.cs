using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Serialization
{
    public static class Text
    {
        // *** Something to text ***
        public static string ToUtcString(this DateTime value) { return value.ToString("yyyyMMddTHHmmssZ"); }

        public static string HexChars = "0123456789abcdef";
        public static char ToHexChar(int value) { return HexChars[value & 0xf]; }

        public static string ToHexString(this byte[] bytes) { return ToHexString(bytes, 0, bytes.Length); }

        public static string ToHexString(this byte[] bytes, int offset, int count)
        {
            char[] hex = new char[count * 2];
            for (var i = 0; i < count; i++)
            {
                hex[i * 2] = ToHexChar(bytes[offset + i] >> 4);
                hex[i * 2 + 1] = ToHexChar(bytes[offset + i]);
            }
            return new string(hex);
        }
             
        public static string ToHexString(this ArraySegment<byte> value)
        {
            return ToHexString(value.Array, value.Offset, value.Count);
        }

        // *** Text to something ***

        public static DateTime ToDateTime(this string text, DateTime defaultUtcDate = default(DateTime))
        {
            if (text == null) return defaultUtcDate;
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
        }

        public static bool ToBool(this string text, bool defaultValue = false)
        {
            if (text == null) return defaultValue;
            var value = text.ToLower();
            if (value == "true") return true;
            if (value == "yes") return true;
            if (value == "y") return true;
            if (value == "false") return false;
            if (value == "no") return false;
            if (value == "n") return false;
            double doubleValue;
            if (double.TryParse(value, out doubleValue)) return doubleValue != 0;
            return defaultValue;
        }

        public static int ToInt(this string text, int defaultValue = 0)
        {
            if (text == null) return defaultValue;
            int.TryParse(text, out defaultValue);
            return defaultValue;
        }

        public static long ToLong(this string text, long defaultValue = 0L)
        {
            long.TryParse(text, out defaultValue);
            return defaultValue;
        }

        public static double ToDouble(this string text, double defaultValue = 0.0)
        {
            if (text == null) return defaultValue;
            double.TryParse(text, out defaultValue);
            return defaultValue;
        }

        public static byte[] ToByteArray(this string text, byte[] defaultValue = null)
        {
            if (text == null) return defaultValue;
            byte[] bytes = new byte[text.Length >> 1];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)((FromHexChar(text[i * 2]) << 4) | FromHexChar(text[i * 2 + 1]));
            return bytes;
        }

        public static int FromHexChar(char c)
        {
            var value = (int)c;
            if (value >= 48 && value <= 57) return value - 48;
            if (value >= 65 && value <= 70) return value - 65 + 10;
            if (value >= 97 && value <= 102) return value - 97 + 10;
            return 0;
        }

        public static ArraySegment<byte> ToByteSegment(this string text)
        {
            return new ArraySegment<byte>(ToByteArray(text));
        }

        public static  Hash ToHash(this string text)
        {
            return Hash.FromText(text);
        }


    }
}

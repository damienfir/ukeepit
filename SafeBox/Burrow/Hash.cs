using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SafeBox.Burrow
{
    public class Hash : IComparable<Hash>
    {
        // *** Object ***

        private string hexHash;
        private byte[] binaryHash;

        internal Hash(string hex, byte[] binary)
        {
            this.hexHash = hex;
            this.binaryHash = binary;
        }

        public string Hex()
        {
            if (hexHash != null) return hexHash;
            hexHash = Static.BytesToHexString(binaryHash);
            return hexHash;
        }

        public string HexWithSpaces()
        {
            var hexHash = Hex();
            return hexHash.Substring(0, 4) + " " + hexHash.Substring(4, 4) + " " + hexHash.Substring(8, 4) + " " + hexHash.Substring(12, 4) + " " +
                hexHash.Substring(16, 4) + " " + hexHash.Substring(20, 4) + " " + hexHash.Substring(24, 4) + " " + hexHash.Substring(28, 4) + " " +
                hexHash.Substring(32, 4) + " " + hexHash.Substring(36, 4) + " " + hexHash.Substring(40, 4) + " " + hexHash.Substring(44, 4) + " " +
                hexHash.Substring(48, 4) + " " + hexHash.Substring(52, 4) + " " + hexHash.Substring(56, 4) + " " + hexHash.Substring(60, 4);
        }

        public byte[] Bytes()
        {
            if (binaryHash != null) return binaryHash;
            binaryHash = Static.HexStringToBytes(hexHash);
            return binaryHash;
        }

        public void WriteToByteArray(byte[] array, int offset)
        {
            var bytes = Bytes();
            Array.Copy(bytes, 0, array, offset, 32);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Hash);
        }

        public bool Equals(Hash hash)
        {
            if (hash == null) return false;
            if (ReferenceEquals(this, hash)) return true;

            // Compare both hex representations if they exist
            if (this.hexHash != null && hash.hexHash != null) return this.hexHash == hash.hexHash;

            // Otherwise, compare the binary representations
            var bytes1 = this.Bytes();
            var bytes2 = hash.Bytes();
            for (var i = 0; i < 32; i++) if (bytes1[i] != bytes2[i]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var bytes = this.Bytes();
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }

        // *** Static ***

        private static Regex hexHashRegex = new Regex(@"^\s*([a-fA-F0-9]{64,64})\s*$");
        private static Regex hexInTextRegex = new Regex(@"([a-fA-F0-9]{64,64})");
        private static Regex hexInText1Regex = new Regex(@"([a-fA-F0-9]{2,2}).([a-fA-F0-9]{62,62})");
        private static Regex hexInText2Regex = new Regex(@"([a-fA-F0-9]{3,3}).([a-fA-F0-9]{61,61})");

        public static Hash From(string hashHex)
        {
            var match = hexHashRegex.Match(hashHex);
            if (match.Success) return new Hash(match.Groups[1].Value, null);
            return null;
        }

        public static Hash From(ArraySegment<byte> bytes)
        {
            return bytes.Count == 32 ? From(bytes.Array, bytes.Offset) : null;
        }

        public static Hash From(byte[] bytes, int offset = 0)
        {
            if (bytes == null || offset + 32 > bytes.Length) return null;
            var copy = new byte[32];
            Array.Copy(bytes, offset, copy, 0, 32);
            return new Hash(null, copy);
        }

        public static Hash FromText(string text)
        {
            var match = hexInTextRegex.Match(text);
            if (match.Success) return new Hash(match.Groups[1].Value, null);
            var match1 = hexInText1Regex.Match(text);
            if (match1.Success) return new Hash(match1.Groups[1].Value + match1.Groups[2].Value, null);
            var match2 = hexInText2Regex.Match(text);
            if (match2.Success) return new Hash(match2.Groups[1].Value + match2.Groups[2].Value, null);
            return null;
        }

        public static Hash For(byte[] bytes)
        {
            var sha256 = new System.Security.Cryptography.SHA256Managed();
            return From(sha256.ComputeHash(bytes));
        }

        public static Hash For(byte[] bytes, int offset, int count)
        {
            var sha256 = new System.Security.Cryptography.SHA256Managed();
            return From(sha256.ComputeHash(bytes, offset, count));
        }

        int IComparable<Hash>.CompareTo(Hash hash)
        {
            if (hash == null) return 64;
            var bytes1 = this.Bytes();
            var bytes2 = hash.Bytes();
            for (var i = 0; i < 32; i++)
            {
                if (bytes1[i] < bytes2[i]) return -(32 - i);
                if (bytes1[i] > bytes2[i]) return 32 - i;
            }
            return 0;
        }

        public static bool operator ==(Hash a, Hash b) { return a.Equals(b); }
        public static bool operator !=(Hash a, Hash b) { return !a.Equals(b); }
    }
}

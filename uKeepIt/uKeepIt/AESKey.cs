using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace uKeepIt
{
    class AESKey
    {
        private static readonly byte[] _salt = new byte[] {0x63, 0xdb, 0xa5, 0x55, 0x48, 0xfa, 0x53, 0x54, 0xff, 0xdc, 0x4e, 0x3f, 0x1d, 0x4b, 0xc8, 0xe0};

        public static byte[] generateKey(string password)
        {
            var keygen = new Rfc2898DeriveBytes(password, _salt);
            return keygen.GetBytes(32);
        }

        public static bool storeKey(byte[] key, string filename)
        {
            return MiniBurrow.Static.WriteFile(filename, key);
        }
    }
}
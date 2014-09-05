using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SafeBox.Burrow
{
    public class PrivateKey
    {
        public static PrivateKey From(Configuration.IniFileSection section)
        {
            if (section == null) return null;

            // Load the key from the dictionary
            var key = new RSAParameters();
            key.Modulus = section.Get("modulus", null as byte[]);
            key.Exponent = section.Get("exponent", null as byte[]);
            key.D = section.Get("d", null as byte[]);
            key.P = section.Get("p", null as byte[]);
            key.Q = section.Get("q", null as byte[]);
            key.DP = section.Get("d mod p", null as byte[]);
            key.DQ = section.Get("d mod q", null as byte[]);
            key.InverseQ = section.Get("inv(q) mod p", null as byte[]);
            if (key.Modulus == null || key.Exponent == null || key.D == null) return null;

            // Load the key from the dictionary
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(key);
            return new PrivateKey(rsa);
        }

        // *** Object ***

        public readonly RSACryptoServiceProvider RSACryptoServiceProvider;

        public PrivateKey(RSACryptoServiceProvider rsaCryptoServiceProvider)
        {
            this.RSACryptoServiceProvider = rsaCryptoServiceProvider;
        }

        public byte[] Encrypt(ArraySegment<byte> bytes) { return RSACryptoServiceProvider.Encrypt(Static.ToByteArray(bytes), true); }
        public byte[] Decrypt(ArraySegment<byte> bytes) { return RSACryptoServiceProvider.Decrypt(Static.ToByteArray(bytes), true); }
        public byte[] Sign(Hash hash) { return RSACryptoServiceProvider.SignHash(hash.Bytes(), "SHA256"); }
        public bool Verify(Hash hash, ArraySegment<byte> signatureBytes) { return RSACryptoServiceProvider.VerifyHash(hash.Bytes(), "SHA256", Static.ToByteArray(signatureBytes)); }
    }

    /*
    public class UnlockedPrivateKey
    {
        public readonly Hash Hash;
        public readonly PrivateKey PrivateKey;
        public UnlockedPrivateKey(Hash hash, PrivateKey privateKey)
        {
            this.Hash = hash;
            this.PrivateKey = privateKey;
        }
    }
     */
}

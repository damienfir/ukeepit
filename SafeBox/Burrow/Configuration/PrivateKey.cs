using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SafeBox.Burrow
{
    public class PrivateKey
    {
        public static PrivateKey From(Configuration.Dictionary dictionary)
        {
            if (dictionary == null) return null;

            // Load the key from the dictionary
            var key = new RSAParameters();
            key.Modulus = dictionary.Get("modulus", null as byte[]);
            key.Exponent = dictionary.Get("exponent", null as byte[]);
            key.D = dictionary.Get("d", null as byte[]);
            key.P = dictionary.Get("p", null as byte[]);
            key.Q = dictionary.Get("q", null as byte[]);
            key.DP = dictionary.Get("d mod p", null as byte[]);
            key.DQ = dictionary.Get("d mod q", null as byte[]);
            key.InverseQ = dictionary.Get("inv(q) mod p", null as byte[]);
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

        public byte[] Encrypt(ArraySegment<byte> bytes) { return RSACryptoServiceProvider.Encrypt(Static.ByteArray(bytes), true); }
        public byte[] Decrypt(ArraySegment<byte> bytes) { return RSACryptoServiceProvider.Decrypt(Static.ByteArray(bytes), true); }
        public byte[] Sign(Hash hash) { return RSACryptoServiceProvider.SignHash(hash.Bytes(), "SHA256"); }
        public bool Verify(Hash hash, ArraySegment<byte> signatureBytes) { return RSACryptoServiceProvider.VerifyHash(hash.Bytes(), "SHA256", Static.ByteArray(signatureBytes)); }
    }

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
}

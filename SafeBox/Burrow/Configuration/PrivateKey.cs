using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow
{
    public class PrivateKey
    {
        public static PrivateKey From(Configuration.IniFileSection section)
        {
            if (section == null) return null;

            // Load the key from the dictionary
            var key = new RSAParameters();
            key.Modulus = section.Get("modulus").ToByteArray();
            key.Exponent = section.Get("exponent").ToByteArray();
            key.D = section.Get("d").ToByteArray();
            key.P = section.Get("p").ToByteArray();
            key.Q = section.Get("q").ToByteArray();
            key.DP = section.Get("d mod p").ToByteArray();
            key.DQ = section.Get("d mod q").ToByteArray();
            key.InverseQ = section.Get("inv(q) mod p").ToByteArray();
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

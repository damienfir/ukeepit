using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SafeBox.Burrow
{
    public class PublicKey
    {
        // *** Static ***

        public static Hash IdentityHashForPublicKeyBytes(byte[] publicKeyBytes)
        {
            // Calculate the identity hash
            var publicKeyObjectBytes = new byte[4 + publicKeyBytes.Length];
            publicKeyObjectBytes[0] = 0;
            publicKeyObjectBytes[1] = 0;
            publicKeyObjectBytes[2] = 0;
            publicKeyObjectBytes[3] = 0;
            System.Array.Copy(publicKeyBytes, 0, publicKeyObjectBytes, 4, publicKeyBytes.Length);
            return Hash.For(publicKeyObjectBytes);
        }

        public static PublicKey From(byte[] bytes) { return From(IdentityHashForPublicKeyBytes(bytes), Configuration.Dictionary.From(bytes)); }
        public static PublicKey From(Hash hash, ArraySegment<byte> bytes) { return From(hash, Configuration.Dictionary.From(bytes)); }

        public static PublicKey From(Hash hash, Configuration.Dictionary dictionary)
        {
            if (dictionary == null) return null;
            var key = new RSAParameters();
            key.Modulus = dictionary.Get("modulus", null as byte[]);
            key.Exponent = dictionary.Get("exponent", null as byte[]);
            if (key.Modulus == null || key.Exponent == null) return null;

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(key);
            return new PublicKey(hash, rsa);
        }

        // *** Object ***

        public readonly Hash Hash;
        public readonly RSACryptoServiceProvider RSACryptoServiceProvider;

        public PublicKey(Hash hash, RSACryptoServiceProvider rsaCryptoServiceProvider)
        {
            this.Hash = hash;
            this.RSACryptoServiceProvider = rsaCryptoServiceProvider;
        }

        public byte[] Encrypt(ArraySegment<byte> bytes) { 
            return RSACryptoServiceProvider.Encrypt(Static.ByteArray(bytes), true); 
        }

        public bool VerifySignature(Hash hash, ArraySegment<byte> signatureBytes) { 
            return RSACryptoServiceProvider.VerifyHash(hash.Bytes(), "SHA256", Static.ByteArray(signatureBytes)); 
        }
    }
}

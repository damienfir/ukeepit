using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt.MiniBurrow.Aes
{
    public class Static
    {
        internal static RNGCryptoServiceProvider SecureRandom = new RNGCryptoServiceProvider();
        internal static AesManaged ecb = CreateEcb();

        private static AesManaged CreateEcb()
        {
            var aes = new AesManaged();
            aes.Mode = System.Security.Cryptography.CipherMode.ECB;
            aes.Padding = System.Security.Cryptography.PaddingMode.None;
            return aes;
        }

        public static byte[] SecureRandomByteArray(int length)
        {
            var bytes = new byte[length];
            SecureRandom.GetBytes(bytes);
            return bytes;
        }

        public static ArraySegment<byte> SecureRandomBytes(int length)
        {
            var bytes = new byte[length];
            SecureRandom.GetBytes(bytes);
            return bytes.ToByteSegment();
        }

        // This is used for encryption and decryption
        public static void Process(ArraySegment<byte> bytes, byte[] key, byte[] nonce) {
            var encryptor = ecb.CreateEncryptor(key, null);

            var counter = new byte[16];
            for (var n = 0; n < 16; n++) counter[n] = nonce[n];
            var encryptedCounter = new byte[16];

            var i = 0;
            while (i < bytes.Count - 16)
            {
                // Process this block
                encryptor.TransformBlock(counter, 0, 16, encryptedCounter, 0);
                for (var n = 0; n < 16; n++) bytes.Array[bytes.Offset + n] ^= encryptedCounter[n];
                i+=16;

                // Increment the counter
                for (var n = 15; n >= 0; n-- )
                {
                    counter[n]++;
                    if (counter[n] != 0) break;
                }
            }

            encryptor.TransformBlock(counter, 0, 16, encryptedCounter, 0);
            for (var n = 0; n < bytes.Count- i; n++) bytes.Array[bytes.Offset + n] ^= encryptedCounter[n];
        }

        public static void Process(ArraySegment<byte> bytes, ArraySegment<byte> key, ArraySegment<byte> nonce) { Process(bytes, key.ToByteArray(), nonce.ToByteArray()); }

        internal static byte[] KDF(ArraySegment<byte> key, ArraySegment<byte> iv, int iterations)
        {
            var encryptor = ecb.CreateEncryptor(key.ToByteArray(), null);
            var result = new byte[32];
            for (var i = 0; i < 32; i++) result[i] = iv.Array[iv.Offset + i];
            for (var i = 0; i < iterations; i++)
            {
                encryptor.TransformBlock(result, 0, 16, result, 0);
                encryptor.TransformBlock(result, 16, 16, result, 16);
            }
            return result;
        }
    }
}
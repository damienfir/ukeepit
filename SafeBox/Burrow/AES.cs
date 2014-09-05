using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow
{
    public class Aes
    {
        internal static AesManaged aes = CreateAESManaged();

        private static AesManaged CreateAESManaged()
        {
            var aes = new AesManaged();
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            return aes;
        }

        public static ArraySegment<byte> Encrypt(ArraySegment<byte> bytes, byte[] key, byte[] iv, int prependBytes = 0) {
            var encryptor = aes.CreateEncryptor(key, iv);
            var blockSize = encryptor.InputBlockSize;
            var encryptedBytes = new byte[bytes.Count + blockSize + prependBytes];

            var i = 0;
            var w = 0;
            while (i < bytes.Count - blockSize)
            {
                w += encryptor.TransformBlock(bytes.Array, bytes.Offset + i, blockSize, encryptedBytes, prependBytes + w);
                i += blockSize;
            }

            var finalBuffer = encryptor.TransformFinalBlock(bytes.Array, bytes.Offset + i, bytes.Count - i);
            for (int n = 0; n < finalBuffer.Length; n++) encryptedBytes[prependBytes + w + n] = finalBuffer[n];
            return new ArraySegment<byte>(encryptedBytes, 0, prependBytes + w + finalBuffer.Length);
        }

        public static AesEncryptedByteSegment Encrypt(byte[] bytes, int prependBytes = 0) { return Encrypt(new ArraySegment<byte>(bytes), prependBytes); }

        public static AesEncryptedByteSegment Encrypt(ArraySegment<byte> bytes, int prependBytes = 0)
        {
            if (bytes == null) return null;

            var key = new byte[32];
            Static.Random.NextBytes(key);
            var iv = new byte[16];
            Static.Random.NextBytes(iv);

            return new AesEncryptedByteSegment(Aes.Encrypt(bytes, key, iv, prependBytes), new ArraySegment<byte>(key), new ArraySegment<byte>(iv));
        }

        public static ArraySegment<byte> Decrypt(ArraySegment<byte> bytes, ArraySegment<byte> key, ArraySegment<byte> iv) { return Decrypt(bytes, Static.ToByteArray(key), Static.ToByteArray(iv)); }

        public static ArraySegment<byte> Decrypt(ArraySegment<byte> bytes, byte[] key, byte[] iv)
        {
            var decryptor = aes.CreateDecryptor(key, iv);
            var blockSize = decryptor.InputBlockSize;
            var decryptedBytes = new byte[bytes.Count];

            var i = 0;
            var w = 0;
            while (i < bytes.Count - blockSize)
            {
                w += decryptor.TransformBlock(bytes.Array, bytes.Offset + i, blockSize, decryptedBytes, w);
                i += blockSize;
            }

            var finalBuffer = decryptor.TransformFinalBlock(bytes.Array, bytes.Offset + i, blockSize);
            for (int n = 0; n < finalBuffer.Length; n++) decryptedBytes[w + n] = finalBuffer[n];
            return new ArraySegment<byte>(decryptedBytes, 0, w + finalBuffer.Length);
        }
    }

    public class AesEncryptedByteSegment
    {
        public readonly ArraySegment<byte> Bytes;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Iv;

        public AesEncryptedByteSegment(ArraySegment<byte> bytes, ArraySegment<byte> aesKey, ArraySegment<byte> aesIv)
        {
            this.Bytes = bytes;
            this.Key = aesKey;
            this.Iv = aesIv;
        }
    }

    public class AesEncryptedObject
    {
        // *** Static ***

        public static AesEncryptedObject For(HashCollector hashCollector, ByteChain byteChain)
        {
            // Prepare the data
            var dataLength = byteChain.ByteLength();
            var dataBytes = new byte[dataLength];
            byteChain.WriteToByteArray(dataBytes, 0);

            // Encrypt the data, while keeping enough space for the header
            var aesEncryptedByteSegment = Aes.Encrypt(dataBytes, hashCollector.ByteLength());

            // Add the header
            hashCollector.WriteToByteArray(aesEncryptedByteSegment.Bytes.Array, aesEncryptedByteSegment.Bytes.Offset);

            // Create the encrypted object
            return new AesEncryptedObject(new Serialization.BurrowObject(aesEncryptedByteSegment.Bytes), aesEncryptedByteSegment.Key, aesEncryptedByteSegment.Iv);
        }

        public static AesEncryptedObject For(byte[] bytes) { return For(new ArraySegment<byte>(bytes)); }

        public static AesEncryptedObject For(ArraySegment<byte> bytes)
        {
            var encryptedByteSegment = Aes.Encrypt(bytes, 4);
            encryptedByteSegment.Bytes.Array[0] = 0;
            encryptedByteSegment.Bytes.Array[1] = 0;
            encryptedByteSegment.Bytes.Array[2] = 0;
            encryptedByteSegment.Bytes.Array[3] = 0;
            return new AesEncryptedObject(new Serialization.BurrowObject(encryptedByteSegment.Bytes), encryptedByteSegment.Key, encryptedByteSegment.Iv);
        }

        // *** Object ***

        public readonly Serialization.BurrowObject Object;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Iv;

        public AesEncryptedObject(Serialization.BurrowObject obj, ArraySegment<byte> aesKey, ArraySegment<byte> aesIv)
        {
            this.Object = obj;
            this.Key = aesKey;
            this.Iv = aesIv;
        }
    }

    public class HashWithAesParameters
    {
        public readonly Hash Hash;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Iv;

        public HashWithAesParameters(Hash hash, ArraySegment<byte> aesKey, ArraySegment<byte> aesIv)
        {
            this.Hash = hash;
            this.Key = aesKey;
            this.Iv = aesIv;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as HashWithAesParameters);
        }

        public bool Equals(HashWithAesParameters that)
        {
            if (that == null) return false;
            if (ReferenceEquals(this, that)) return true;
            return Hash.Equals(that.Hash) && Static.IsEqual(Key, that.Key) && Static.IsEqual(Iv, that.Iv);
        }

        // We do not expect the same hash with distinct AES keys.
        public override int GetHashCode() { return Hash.GetHashCode(); }
    }
}

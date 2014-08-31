using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow
{
    public class AESEncryptedByteSegment
    {
        // *** Static ***
        static AesManaged aes = CreateAESManaged();

        private static AesManaged CreateAESManaged()
        {
            var aes = new AesManaged();
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            return aes;
        }

        public static AESEncryptedByteSegment Encrypt(byte[] bytes, int offset, int length, int prependBytes)
        {
            if (bytes == null) return null;

            var key = new byte[32];
            Static.Random.NextBytes(key);
            var iv = new byte[16];
            Static.Random.NextBytes(iv);

            var encryptor = aes.CreateEncryptor(key, iv);
            var blockSize = encryptor.InputBlockSize;
            var encryptedBytes = new byte[length + blockSize + prependBytes];

            var i = 0;
            var w = 0;
            while (i < length - blockSize)
            {
                w+=encryptor.TransformBlock(bytes, offset + i, blockSize, encryptedBytes, prependBytes + w);
                i += blockSize;
            }

            var finalBuffer = encryptor.TransformFinalBlock(bytes, offset + i, length - i);
            for (int n = 0; n < finalBuffer.Length; n++) encryptedBytes[prependBytes + w + n] = finalBuffer[n];

            return new AESEncryptedByteSegment(new ArraySegment<byte>(encryptedBytes, 0, prependBytes + w + finalBuffer.Length), new ArraySegment<byte>(key), new ArraySegment<byte>(iv));
        }

        // *** Object ***

        public readonly ArraySegment<byte> Bytes;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Iv;

        public AESEncryptedByteSegment(ArraySegment<byte> bytes, ArraySegment<byte> aesKey, ArraySegment<byte> aesIv)
        {
            this.Bytes = bytes;
            this.Key = aesKey;
            this.Iv = aesIv;
        }

        public ArraySegment<byte> Decrypt()
        {
            if (Key == null || Iv == null || Bytes == null) return new ArraySegment<byte>(null, 0, 0);

            var keyArray = new byte[32];
            var ivArray = new byte[16];
            System.Array.Copy(Key.Array, Key.Offset, keyArray, 0, 32);
            System.Array.Copy(Iv.Array, Iv.Offset, ivArray, 0, 32);
            var decryptor = aes.CreateDecryptor(keyArray, ivArray);
            var blockSize = decryptor.InputBlockSize;
            var decryptedBytes = new byte[Bytes.Count];

            var i = 0;
            var w = 0;
            while (i < Bytes.Count - blockSize)
            {
                w += decryptor.TransformBlock(Bytes.Array, Bytes.Offset + i, blockSize, decryptedBytes, w);
                i += blockSize;
            }

            var finalBuffer = decryptor.TransformFinalBlock(Bytes.Array, Bytes.Offset + i, blockSize);
            for (int n = 0; n < finalBuffer.Length; n++) decryptedBytes[w + n] = finalBuffer[n];
            return new ArraySegment<byte>(decryptedBytes, 0, w + finalBuffer.Length);
        }
    }

    public class AESEncryptedObject
    {
        // *** Static ***

        public static AESEncryptedObject From(HashCollector hashCollector, ByteChain byteChain)
        {
            // Prepare the data
            var dataLength = byteChain.ByteLength();
            var dataBytes = new byte[dataLength];
            byteChain.WriteToByteArray(dataBytes, 0);

            // Encrypt the data, while keeping enough space for the header
            var aesEncryptedByteSegment = AESEncryptedByteSegment.Encrypt(dataBytes, 0, dataLength, hashCollector.ByteLength());

            // Add the header
            hashCollector.WriteToByteArray(aesEncryptedByteSegment.Bytes.Array, aesEncryptedByteSegment.Bytes.Offset);

            // Create the encrypted object
            return new AESEncryptedObject(new Serialization.BurrowObject(aesEncryptedByteSegment.Bytes), aesEncryptedByteSegment.Key, aesEncryptedByteSegment.Iv);
        }

        public static AESEncryptedObject From(byte[] bytes, int offset, int length)
        {
            var encryptedByteSegment = AESEncryptedByteSegment.Encrypt(bytes, offset, length, 4);
            encryptedByteSegment.Bytes.Array[0] = 0;
            encryptedByteSegment.Bytes.Array[1] = 0;
            encryptedByteSegment.Bytes.Array[2] = 0;
            encryptedByteSegment.Bytes.Array[3] = 0;
            return new AESEncryptedObject(new Serialization.BurrowObject(encryptedByteSegment.Bytes), encryptedByteSegment.Key, encryptedByteSegment.Iv);
        }

        // *** Object ***

        public readonly Serialization.BurrowObject Object;
        public readonly ArraySegment<byte> Key;
        public readonly ArraySegment<byte> Iv;

        public AESEncryptedObject(Serialization.BurrowObject obj, ArraySegment<byte> aesKey, ArraySegment<byte> aesIv)
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

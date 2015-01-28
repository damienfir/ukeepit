using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow.Serialization;

namespace uKeepIt.MiniBurrow.Aes
{
    class Envelope
    {
        public static BurrowObject Create(ObjectReference objectReference, ArraySegment<byte> key)
        {
            // Create serialization structures
            var hashCollector = new ObjectHeader();

            // Create an envelope
            var envelope = new DictionaryConstructor();
            envelope.Add("content", objectReference.Hash);
            envelope.Add("aes nonce", objectReference.Nonce);

            // Derive a unique mask
            var maskIv = MiniBurrow.Static.RandomBytes(32);
            envelope.Add("mask iv", maskIv);
            var mask = Static.KDF(key, maskIv, 1);
            var verificationMask = Static.KDF(key, maskIv, 2);
            envelope.Add("verification mask", verificationMask);

            // Mask the AES key
            var maskedAesKey = new byte[32];
            var offset = objectReference.Key.Offset;
            for (var i = 0; i < 32; i++) maskedAesKey[i] = (byte)(mask[i] ^ objectReference.Key.Array[offset + i]);
            envelope.Add("masked aes key", maskedAesKey);
            return envelope.ToBurrowObject();
        }

        private static byte[] EncryptedAESKeyFor(Hash hash)
        {
            var bytes = new ByteWriter();
            bytes.Append("rsa encrypted aes key for ");
            bytes.Append(hash.Bytes());
            return bytes.ToByteArray();
        }

        public static ObjectReference Open(BurrowObject obj, ArraySegment<byte> key)
        {
            var envelope = Dictionary.From(obj);

            // Read the envelope
            var contentHash = envelope.Get("content").AsHash();
            if (contentHash == null) return null;
            var aesNonce = envelope.Get("aes nonce").AsBytes();
            if (aesNonce == null || aesNonce.Count != 16) return null;
            var maskedAesKey = envelope.Get("masked aes key").AsBytes();
            if (maskedAesKey == null || maskedAesKey.Count!= 32) return null;
            var maskIv = envelope.Get("mask iv").AsBytes();
            if (maskIv == null || maskIv.Count != 32) return null;

            // Derive the mask, check the verification mask
            var mask = Static.KDF(key, maskIv, 1);
            var verificationMask = envelope.Get("verification mask").AsBytes();
            var calculatedVerificationMask = Static.KDF(key, maskIv, 2).ToByteSegment();
            if (verificationMask != null && verificationMask.Count == 32 && !MiniBurrow.Static.IsEqual(verificationMask, calculatedVerificationMask)) return null;

            // Unmask the key
            var aesKey = new byte[32];
            for (var i = 0; i < 32; i++) aesKey[i] = (byte)(mask[i] ^ maskedAesKey.Array[maskedAesKey.Offset+ i]);
            return new ObjectReference(contentHash, aesKey.ToByteSegment(), aesNonce);
        }
    }
}

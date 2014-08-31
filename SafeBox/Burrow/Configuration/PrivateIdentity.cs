using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafeBox.Burrow
{
    public class UnlockedPrivateIdentity
    {
        public readonly PrivateIdentity PrivateIdentity;
        public readonly PrivateKey PrivateKey;

        public UnlockedPrivateIdentity(PrivateIdentity privateIdentity, PrivateKey privateKey)
        {
            this.PrivateIdentity = privateIdentity;
            this.PrivateKey = privateKey;
        }
    }

    public class PrivateIdentity
    {
        public readonly PublicIdentity PublicIdentity;
        public readonly ArraySegment<byte> PublicKeyBytes;
        public readonly Configuration.Dictionary PrivateInformation;

        public PrivateIdentity(PublicIdentity publicIdentity, ArraySegment<byte> publicKeyBytes, Configuration.Dictionary privateInformation)
        {
            this.PublicIdentity = publicIdentity;
            this.PublicKeyBytes = publicKeyBytes;
            this.PrivateInformation = privateInformation;
        }

        // You need to reload the burrow configuration after unlocking a key to get a new snapshot with the unlocked key.
        public UnlockedPrivateIdentity Unlock(byte[] aesKey)
        {
            var encryptedKeyBytes = PrivateInformation.Get("aes encrypted rsa key", null as byte[]);
            var aesIv = PrivateInformation.Get("aes iv", null as byte[]);
            var aesEncryptedByteSegment = new AESEncryptedByteSegment(new ArraySegment<byte>(encryptedKeyBytes), new ArraySegment<byte>(aesKey), new ArraySegment<byte>(aesIv));
            var plainKeyByteSegment = aesEncryptedByteSegment.Decrypt();
            var privateKey = PrivateKey.From(Configuration.Dictionary.From(plainKeyByteSegment));
            if (privateKey==null) return null;
            return new UnlockedPrivateIdentity(this, privateKey);
        }

        public UnlockedPrivateIdentity Unlock()
        {
            // Use a plain RSA key if available
            var plainKeyBytes = PrivateInformation.Get("plain rsa key", null as byte[]);
            var privateKey = PrivateKey.From(Configuration.Dictionary.From(plainKeyBytes));
            if (privateKey != null) return new UnlockedPrivateIdentity(this, privateKey);

            // Try using this dictionary itself as a private key
            privateKey = PrivateKey.From(PrivateInformation);
            if (privateKey != null) return new UnlockedPrivateIdentity(this, privateKey);

            return null;
        }

        internal string CommonName()
        {
            var name = PrivateInformation.Get("name", "");
            if (name != "") return name;
            name = PublicIdentity.PublicInformation.Get("fn", "");
            if (name != "") return name;
            name = (PublicIdentity.PublicInformation.Get("n first", "") + " " + PublicIdentity.PublicInformation.Get("n last", "")).Trim();
            if (name != "") return name;
            return "You";
        }
    }
}

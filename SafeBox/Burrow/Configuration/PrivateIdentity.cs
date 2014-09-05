using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafeBox.Burrow.Configuration
{
    /*
    public class UnlockedPrivateIdentity
    {
        public readonly PrivateIdentity PrivateIdentity;
        public readonly PrivateKey PrivateKey;

        public UnlockedPrivateIdentity(PrivateIdentity privateIdentity, PrivateKey privateKey)
        {
            this.PrivateIdentity = privateIdentity;
            this.PrivateKey = privateKey;
        }
    } */

    public class PrivateIdentity
    {
        //public readonly long Date;
        public readonly IniFile Configuration;
        public readonly ImmutableDictionary<string, string> PublicInformation;
        public readonly ImmutableDictionary<string, string> PrivateInformation;
        public readonly PublicKey PublicKey;
        public readonly ArraySegment<byte> PublicKeyBytes;
        public readonly ImmutableStack<PublicKey> PublicKeys;
        public readonly PrivateKey PrivateKey;  // may be null (if not available)

        public PrivateIdentity(IniFile configuration, PublicKey publicKey, ArraySegment<byte> publicKeyBytes, PrivateKey PrivateKey)
        {
            this.Configuration = configuration;
            this.PublicInformation = configuration.SectionAsImmutableDictionary("public");
            this.PrivateInformation = configuration.SectionAsImmutableDictionary("private");
            this.PublicKey = publicKey;
            this.PublicKeyBytes = publicKeyBytes;
        }

        /*
        // You need to reload the burrow configuration after unlocking a key to get a new snapshot with the unlocked key.
        public UnlockedPrivateIdentity Unlock(byte[] aesKey)
        {
            var privateKeyInfo = Configuration.Section("private key");
            var encryptedKeyBytes = Configuration.Get("aes encrypted rsa key", null as byte[]);
            var aesIv = PrivateInformation.Get("aes iv", null as byte[]);
            var aesEncryptedByteSegment = new AESEncryptedByteSegment(new ArraySegment<byte>(encryptedKeyBytes), new ArraySegment<byte>(aesKey), new ArraySegment<byte>(aesIv));
            var plainKeyByteSegment = aesEncryptedByteSegment.Decrypt();
            var privateKey = PrivateKey.From(Configuration.IniFileSection.From(plainKeyByteSegment));
            if (privateKey==null) return null;
            return new UnlockedPrivateIdentity(this, privateKey);
        }

        public UnlockedPrivateIdentity Unlock()
        {
            // Use a plain RSA key if available
            var plainKeyBytes = PrivateInformation.Get("plain rsa key", null as byte[]);
            var privateKey = PrivateKey.From(Configuration.IniFileSection.From(plainKeyBytes));
            if (privateKey != null) return new UnlockedPrivateIdentity(this, privateKey);

            // Try using this dictionary itself as a private key
            privateKey = PrivateKey.From(PrivateInformation);
            if (privateKey != null) return new UnlockedPrivateIdentity(this, privateKey);

            return null;
        } */

        internal string CommonName()
        {
            var name = PrivateInformation.Get("name");
            if (name != "") return name;
            name = PublicInformation.Get("name");
            if (name != "") return name;
            name = (PublicInformation.Get("first name") + " " + PublicInformation.Get("last name")).Trim();
            if (name != "") return name;
            return "You";
        }
    }
}

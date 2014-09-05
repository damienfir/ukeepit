using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Backend;

namespace SafeBox.Burrow
{
    public class PublicIdentity
    {
        public readonly PublicKey PublicKey;
        public readonly long Date;
        public readonly Dictionary PublicInformation;
        public readonly ImmutableStack<PublicKey> PublicKeys;
        public readonly ImmutableStack<AccountStore> AccountStores;

        public PublicIdentity(PublicKey publicKey, long date, Dictionary publicInformation, ImmutableStack<PublicKey> publicKeys, ImmutableStack<AccountStore> accountStores)
        {
            this.PublicKey = publicKey;
            this.Date = date;
            this.PublicInformation = publicInformation;
            this.PublicKeys = publicKeys;
            this.AccountStores = accountStores;
        }

        internal string CommonName()
        {
            var name = PublicInformation.Get("fn", "");
            if (name != "") return name;
            name = (PublicInformation.Get("n first", "") + " " + PublicInformation.Get("n last", "")).Trim();
            if (name != "") return name;
            return "Somebody";
        }
    }
}

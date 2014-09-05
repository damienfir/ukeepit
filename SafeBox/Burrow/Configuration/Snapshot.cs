using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;

namespace SafeBox.Burrow.Configuration
{
    public class Snapshot
    {
        public readonly ImmutableDictionary<Hash, PrivateIdentity> Identities;
        public readonly ImmutableStack<ObjectStore> PermanentObjectStores;
        public readonly ImmutableStack<AccountStore> PermanentAccountStores;
        public readonly ImmutableStack<ObjectStore> ObjectStores;
        public readonly ImmutableStack<AccountStore> AccountStores;
        public readonly DateTime UtcCreation = DateTime.UtcNow;

        public Snapshot(ImmutableDictionary<Hash,PrivateIdentity> identities, ImmutableStack<ObjectStore> permanentObjectStores, ImmutableStack<AccountStore> permanantAccountStores, ImmutableStack<ObjectStore> objectStores, ImmutableStack<AccountStore> accountStores)
        {
            this.PermanentObjectStores = permanentObjectStores;
            this.PermanentAccountStores = permanantAccountStores;
            this.ObjectStores = objectStores;
            this.AccountStores = accountStores;
            this.Identities = identities;
        }


        /*
        public List<AbstractAccount> AccountsForIdentityHash(Hash identityHash)
        {
            var accounts = new List<AbstractAccount>();
            foreach (var store in Stores)
            {
                var account = store.Account(identityHash);
                if (account == null) continue;
                accounts.Add(account);
            }
            return accounts;
        }

        public Dictionary<string, SerializedObject> PublicKeysForIdentityHash(Hash identityHash)
        {
            var publicKeys = new Dictionary<string, SerializedObject>();
            foreach (var account in AccountsForIdentityHash(identityHash))
            {
                var accountIdentity = AccountIdentity(account);
                if (accountIdentity == null) continue;
                foreach (var hash in accountIdentity.PublicKeyHashes())
                {
                    if (publicKeys.ContainsKey(hash.Hex())) continue;
                    publicKeys[hash.Hex()] = GetObject(hash);
                }
            }
            return publicKeys;
        }*/
    }
}

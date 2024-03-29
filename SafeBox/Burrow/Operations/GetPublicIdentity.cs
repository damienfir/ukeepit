﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Operations
{
    public class GetPublicIdentity
    {
        public delegate void Done(PublicIdentity publicIdentity);
        internal readonly Cache cache;
        internal readonly ObjectStore ObjectStore;
        internal readonly AccountStore AccountStore;
        internal readonly Hash Hash;
        internal GetPublicKey GetPublicKey;
        public readonly TaskGroup.Future<PublicIdentity> Future;

        public GetPublicIdentity(Hash hash, Cache cache, AccountStore accountStore, ObjectStore objectStore, bool forceReload, TaskGroup taskGroup)
        {
            this.Future = taskGroup.WaitForMe<PublicIdentity>();
            this.AccountStore = accountStore;
            this.ObjectStore = objectStore;
            this.cache = cache;
            this.Hash = hash;

            // Load from cache
            if (!forceReload)
            {
                var publicIdentity = null as PublicIdentity;
                if (cache.PublicIdentitiesByHash.TryGetValue(hash, out publicIdentity)) { Future.Done(publicIdentity); return; }
            }

            // Retrieve the main public key
            var tg = new TaskGroup();
            GetPublicKey = new GetPublicKey(hash, cache, objectStore, tg);
            tg.WhenDone(GetPublicKeyDone);
        }

        private void GetPublicKeyDone()
        {
            if (GetPublicKey.Future.Result == null) { Future.Done(null); return; }

            // Retrieve the account identity from all stores
            AccountStore.
            foreach (var store in AccountStores)
                new GetPublicIdentityFromAccount(this, Hash);
        }

        private long NewestPublicInformationDate = long.MinValue;
        private Dictionary NewestPublicInformationDictionary = null;
        private ImmutableStack<AccountStore> NewestPublicInformationStores = new ImmutableStack<AccountStore>();
        internal void PublicInformationCandidate(Dictionary publicInformation, AccountStore store)
        {
            var registeredDate = publicInformation.Get("reg").AsLong();
            if (NewestPublicInformationDictionary == null || NewestPublicInformationDate < registeredDate)
            {
                NewestPublicInformationDate = registeredDate;
                NewestPublicInformationDictionary = publicInformation;
                NewestPublicInformationStores = new ImmutableStack<AccountStore>(store);
            }
            else if (NewestPublicInformationDate == registeredDate)
            {
                NewestPublicInformationStores = NewestPublicInformationStores.With(store);
            }
        }

        private int expectedAccounts = 0;
        internal void AccountDone()
        {
            expectedAccounts -= 1;
            if (expectedAccounts > 0) return;

            // Retrieve all associated public keys
            var identities = NewestPublicInformationDictionary.Get("identities").AsBytes();
            expectedAssociatedPublicKeys = identities.Count / 32;
            if (expectedAssociatedPublicKeys == 0) { WrapUp(); return; }
            for (var i = 0; i < identities.Count; i += 32)
                new GetPublicKey(Hash.From(identities.Array, identities.Offset + i), cache, ObjectStores, GetAssociatedPublicKeyDone);
        }

        private int expectedAssociatedPublicKeys = 0;
        private ImmutableStack<PublicKey> associatedPublicKeys = new ImmutableStack<PublicKey>();
        private void GetAssociatedPublicKeyDone(PublicKey publicKey)
        {
            if (publicKey != null) associatedPublicKeys = associatedPublicKeys.With(publicKey);
            expectedAssociatedPublicKeys -= 1;
            if (expectedAssociatedPublicKeys == 0) WrapUp();
        }

        private void WrapUp()
        {
            // Create and cache this public identity
            var publicIdentity = new PublicIdentity(PublicKey, NewestPublicInformationDate, NewestPublicInformationDictionary, associatedPublicKeys, NewestPublicInformationStores);
            cache.PublicIdentitiesByHash[Hash] = publicIdentity;
            handler(publicIdentity);
        }
    }

    internal class GetPublicIdentityFromAccount
    {
        internal readonly GetPublicIdentity GetPublicIdentity;
        internal readonly Hash IdentityHash;

        public GetPublicIdentityFromAccount(GetPublicIdentity getPublicIdentity, Hash identityHash)
        {
            this.GetPublicIdentity = getPublicIdentity;
            this.IdentityHash = identityHash;

            // List the identity root
            Asynchronous.Run(() => GetPublicIdentity.AccountStore.List(identityHash, "identity"), IdentityRootListDone);
        }

        private int expected = 0;
        void IdentityRootListDone(IEnumerable<ObjectUrl> list)
        {
            // If the root is empty, give up
            expected = list.Count();
            if (expected == 0) { GetPublicIdentity.AccountDone(); return; }

            // If there are >= 1 items, continue loading the envelopes, and keep the newest with a valid signature.
            foreach (var objectUrl in list)
                new GetPublicIdentityFromObjectUrl(this, objectUrl);
        }

        internal void ObjectUrlDone()
        {
            expected -= 1;
            if (expected == 0) GetPublicIdentity.AccountDone();
        }
    }

    internal class GetPublicIdentityFromObjectUrl
    {
        internal readonly GetPublicIdentityFromAccount GetPublicIdentityFromAccount;
        internal readonly ObjectUrl ObjectUrl;
        internal ImmutableStack<ObjectStore> ObjectStores;
        internal Hash contentHash = null;

        public GetPublicIdentityFromObjectUrl(GetPublicIdentityFromAccount getPublicIdentityFromAccount, ObjectUrl objectUrl)
        {
            this.GetPublicIdentityFromAccount = getPublicIdentityFromAccount;
            this.ObjectUrl = objectUrl;

            ObjectStores = GetPublicIdentityFromAccount.GetPublicIdentity.ObjectStores;
            if (objectUrl.Url != null && objectUrl.Url.Trim().Length > 0) ObjectStores = ObjectStores.With(Static.ObjectStoreForUrl(objectUrl.Url, "GetPublicIdentity"));
            new GetFromAnyStore(objectUrl.Hash, ObjectStores, GetEnvelopeDone);
        }

        private void GetEnvelopeDone(BurrowObject obj, ObjectStore store)
        {
            if (obj == null) { GetPublicIdentityFromAccount.ObjectUrlDone(); return; }
            var envelope = Dictionary.From(obj);

            // Verify the signature of the envelope
            var contentHash = envelope.Get("content").AsHash();
            if (contentHash == null) { GetPublicIdentityFromAccount.ObjectUrlDone(); return; }
            var signature = envelope.Get("signature").AsBytes();
            if (signature == null) { GetPublicIdentityFromAccount.ObjectUrlDone(); return; }
            if (!GetPublicIdentityFromAccount.GetPublicIdentity.PublicKey.VerifySignature(contentHash, signature)) { GetPublicIdentityFromAccount.ObjectUrlDone(); return; }

            // Read the identity object
            new GetFromAnyStore(contentHash, ObjectStores.With(store), GetPublicInformationDone);
        }

        private void GetPublicInformationDone(BurrowObject obj, ObjectStore store)
        {
            var publicInformation = Dictionary.From(obj);
            GetPublicIdentityFromAccount.GetPublicIdentity.PublicInformationCandidate(publicInformation, GetPublicIdentityFromAccount.Account.Store);
            GetPublicIdentityFromAccount.ObjectUrlDone();
        }
    }

    public class AccountIdentity
    {
        public readonly Dictionary PublicInformation;
        public readonly Hash Hash;
        public readonly DateTime RegisteredDate;

        public AccountIdentity(Hash hash, Dictionary publicInformation, DateTime registeredDate)
        {
            this.Hash = hash;
            this.PublicInformation = publicInformation;
            this.RegisteredDate = registeredDate;
        }
    }
}

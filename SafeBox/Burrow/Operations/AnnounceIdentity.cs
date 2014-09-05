using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Backend;
using SafeBox.Burrow.Serialization;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Operations
{
    public class AnnounceIdentity
    {
        public delegate void Done(bool success);
        private Done handler;
        private PrivateIdentity identity;
        private Backend.Root root;

        public AnnounceIdentity(PrivateIdentity identity, ObjectStore objectStore, AccountStore accountStore, DateTime registeredDate, Done handler)
        {
            this.identity = identity;
            this.handler = handler;

            // Create the account (if necessary)
            var accountId = identity.PublicKey.Hash;
            var account = accountStore.Account(accountId);
            if (account == null) { if (handler != null) handler(false); return; }

            // Prepare the public information dictionary
            var publicInformation = new DictionaryConstructor();
            foreach (var pair in identity.PublicInformation) publicInformation.Add(pair.Item1, pair.Item2);
            publicInformation.Add("registered date", registeredDate);
            publicInformation.Add("key", identity.PublicKey.Hash);

            // Add the public key
            publicKeyExpectedHash = identity.PublicKey.Hash;
            objectStore.PutObject(BurrowObject.For(new HashCollector(), identity.PublicKeyBytes), identity, PublicKeyPutDone);

            // Store the public information
            var publicInformationHashes = new HashCollector();
            var publicInformationObject = BurrowObject.For(publicInformationHashes, publicInformation.Serialize(publicInformationHashes));
            publicInformationExpectedHash = publicInformationObject.Hash();
            objectStore.PutObject(publicInformationObject, identity, PublicInformationPutDone);

            // Create an envelope with a signature
            var envelope = new DictionaryConstructor();
            envelope.Add("signature", identity.PrivateKey.Sign(publicInformationExpectedHash));
            envelope.Add("content", publicInformationExpectedHash);
            var envelopeHashes = new HashCollector();
            var envelopeObject = BurrowObject.For(envelopeHashes, envelope.Serialize(envelopeHashes));
            envelopeExpectedHash = envelopeObject.Hash();
            objectStore.PutObject(envelopeObject, identity, EnvelopePutDone);

            // Get all existing hashes
            root = account.Root("identity");
            root.List(IdentityRootListDone);
        }

        private bool publicKeyPutDone = false;
        private Hash publicKeyHash = null;
        private Hash publicKeyExpectedHash = null;
        private void PublicKeyPutDone(Hash hash) { publicKeyPutDone = true; publicKeyHash = hash; Continue(); }

        private bool publicInformationPutDone = false;
        private Hash publicInformationHash = null;
        private Hash publicInformationExpectedHash = null;
        private void PublicInformationPutDone(Hash hash) { publicInformationPutDone = true; publicInformationHash = hash; Continue(); }

        private bool envelopePutDone = false;
        private Hash envelopeHash = null;
        private Hash envelopeExpectedHash = null;
        private void EnvelopePutDone(Hash hash) { envelopePutDone = true; envelopeHash = hash; Continue(); }

        private bool identityRootListDone = false;
        private IEnumerable<ObjectUrl> identityRootList = null;
        private void IdentityRootListDone(IEnumerable<ObjectUrl> list) { identityRootListDone = true; identityRootList = list; Continue(); }

        private void Continue()
        {
            // Check if some operations are still pending
            if (!publicKeyPutDone || !publicInformationPutDone || !envelopePutDone || !identityRootListDone) return;

            // Check if all hashes match
            if (!publicKeyExpectedHash.Equals(publicKeyHash) || !publicInformationExpectedHash.Equals(publicInformationHash) || !envelopeExpectedHash.Equals(envelopeHash))
            {
                if (handler != null) handler(false);
                return;
            }

            // Post the new root
            var removeList = identityRootList.Select(item => item.Hash);
            var addList = new List<ObjectUrl>();
            addList.Add(new ObjectUrl(envelopeHash));
            root.Post(addList, removeList, identity, RootPostDone);
        }

        private void RootPostDone(bool success) { if (handler != null) handler(success); }
    }
}

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
        public readonly TaskGroup.Future<bool> Future;
        public readonly PrivateIdentity Identity;
        public readonly AccountStore AccountStore;

        private Hash publicKeyExpectedHash;
        private Hash publicInformationExpectedHash;
        private Hash envelopeExpectedHash;
        private readonly TaskGroup.Future<Hash> publicKeyPut;
        private readonly TaskGroup.Future<Hash> publicInformationPut;
        private readonly TaskGroup.Future<Hash> envelopePut;
        private readonly TaskGroup.Future<IEnumerable<ObjectUrl>> identityRootList;

        public AnnounceIdentity(PrivateIdentity identity, ObjectStore objectStore, AccountStore accountStore, DateTime registeredDate, TaskGroup taskGroup)
        {
            this.Identity = identity;
            this.Future = taskGroup.WaitForMe<bool>();
            this.AccountStore = accountStore;

            // Prepare the public information dictionary
            var publicInformation = new DictionaryConstructor();
            foreach (var pair in identity.PublicInformation) publicInformation.Add(pair.Key, pair.Value);
            publicInformation.Add("revision", registeredDate);
            publicInformation.Add("key", identity.PublicKey.Hash);

            // Add the public key
            var toContinue = new TaskGroup();
            publicKeyExpectedHash = identity.PublicKey.Hash;
            publicKeyPut = toContinue.Run(() => objectStore.Put(BurrowObject.For(new ObjectHeader(), identity.PublicKeyBytes), identity));
            publicKeyPut = objectStore.Put(BurrowObject.For(new ObjectHeader(), identity.PublicKeyBytes), identity, toContinue);

            // Store the public information
            var publicInformationObject = publicInformation.ToBurrowObject();
            publicInformationExpectedHash = publicInformationObject.Hash();
            publicInformationPut = toContinue.Run(() => objectStore.Put(publicInformationObject, identity));

            // Create an envelope with a signature
            var envelope = new DictionaryConstructor();
            envelope.Add("signature", identity.PrivateKey.Sign(publicInformationExpectedHash));
            envelope.Add("content", publicInformationExpectedHash);
            var envelopeObject = envelope.ToBurrowObject();
            envelopeExpectedHash = envelopeObject.Hash();
            envelopePut = toContinue.Run(() => objectStore.Put(envelopeObject, identity));

            // Get all existing hashes
            identityRootList = toContinue.Run(() => accountStore.List(identity.PublicKey.Hash, "identity"));
            toContinue.WhenDone(Continue);
        }
        
        private void Continue()
        {
            // Check if all hashes match
            if (!publicKeyExpectedHash.Equals(publicKeyPut.Result) || !publicInformationExpectedHash.Equals(publicInformationPut.Result) || !envelopeExpectedHash.Equals(envelopePut.Result))
            {
                Future.Done(false);
                return;
            }

            // Post the new root
            var removeList = identityRootList.Result.Select(item => item.Hash);
            var addList = new List<ObjectUrl>();
            addList.Add(new ObjectUrl(envelopeExpectedHash));

            Asynchronous.Run(() => AccountStore.Modify(Identity.PublicKey.Hash, "identity", addList, removeList, Identity), Finish);
        }

        private void Finish(bool success) {
            Future.Done(true);
        }
    }
}

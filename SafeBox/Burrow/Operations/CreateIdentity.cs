using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SafeBox.Burrow.Operations
{
    // Note: you need to call "Reload" on the burrow configuration after this for the identity to appear
    class CreateIdentity
    {
        public delegate void Done(Hash hash);
        private Done handler;

        public CreateIdentity(Configuration configuration, Serialization.Dictionary privateInformation, Serialization.Dictionary publicInformation, Done handler)
        {
            this.handler = handler;
            ThreadPool.QueueUserWorkItem(new WaitCallback(obj => CreateIdentityAsync(configuration, privateInformation, publicInformation)));
        }

        public void CreateIdentityAsync(Configuration configuration, Serialization.Dictionary privateInformation, Serialization.Dictionary publicInformation)
        {
            // Create a key pair
            Static.Log.Info("Creating a key pair");
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
            var key = rsa.ExportParameters(true);
            var publicKey = new Serialization.Dictionary();
            publicKey.Set("modulus", key.Modulus);
            publicKey.Set("exponent", key.Exponent);
            var publicKeyBytes = publicKey.ToBytes();
            var identityHash = Hash.For(publicKeyBytes);

            var privateKey = new Serialization.Dictionary();
            privateKey.Set("modulus", key.Modulus);
            privateKey.Set("exponent", key.Exponent);
            privateKey.Set("d", key.D);
            privateKey.Set("p", key.P);
            privateKey.Set("q", key.Q);
            privateKey.Set("d mod p", key.DP);
            privateKey.Set("d mod q", key.DQ);
            privateKey.Set("inv(q) mod p", key.InverseQ);

            privateInformation.Set("plain rsa key", privateKey.ToBytes());
            var privateInformationBytes = privateInformation.ToBytes();

            // Save
            var identityName = identityHash.Hex().Substring(0, 16);
            Static.Log.Info("Creating the directory '" + identityName + "'");
            var folder = configuration.Folder + "\\identities\\" + identityName;
            if (Static.DirectoryCreate(folder) && 
                Static.WriteFile(folder + "\\public-key", new ArraySegment<byte>(publicKeyBytes)) && 
                Static.WriteFile(folder + "\\private-information", new ArraySegment<byte>(privateInformationBytes)))
            {
                Static.WriteFile(folder + "\\public-information", new ArraySegment<byte>(publicInformation.ToBytes()));
                //Static.WriteFile(folder + "\\raw-private-key", new ArraySegment<byte>(privateKey.ToBytes()));
                Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(identityHash)), null);
                return;
            }

            Static.SynchronizationContext.Post(new SendOrPostCallback(obj => handler(null)), null);
        }
    }
}

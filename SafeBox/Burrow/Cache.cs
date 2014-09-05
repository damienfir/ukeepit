using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow
{
    // Any instance must be used on a single thread only.
    class Cache
    {
        public Dictionary<Hash, PublicKey> PublicKeysByHash = new Dictionary<Hash, PublicKey>();
        public Dictionary<Hash, PublicIdentity> PublicIdentitiesByHash = new Dictionary<Hash, PublicIdentity>();
        public Dictionary<Hash, PrivateKey> PrivateKeysByHash = new Dictionary<Hash, PrivateKey>();
    }
}

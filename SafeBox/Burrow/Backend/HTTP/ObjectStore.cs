using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Backend.HTTP
{
    class ObjectStore : Backend.ObjectStore
    {
        public static ObjectStore ForUrl(string url) { return null; }

        public ObjectStore(string url) : base(url, 50) { }

        public override bool Has(Hash hash)
        {
            throw new NotImplementedException();
        }

        public override BurrowObject Get(Hash hash)
        {
            throw new NotImplementedException();
        }

        public override Hash Put(BurrowObject serializedObject, PrivateIdentity identity)
        {
            throw new NotImplementedException();
        }
    }
}

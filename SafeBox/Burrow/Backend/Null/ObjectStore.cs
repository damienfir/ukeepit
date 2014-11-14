using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;
using SafeBox.Burrow.Serialization;

namespace SafeBox.Burrow.Backend.Null
{
    public class ObjectStore : Backend.ObjectStore
    {
        public ObjectStore(string url) : base(url, 100) { }

        public override bool Has(Hash hash) { return false; }
        public override BurrowObject Get(Hash hash) { return null; }
        public override Hash Put(BurrowObject serializedObject, PrivateIdentity identity) { return null; }
    }
}

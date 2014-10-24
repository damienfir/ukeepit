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

        public override void HasObject(Hash hash, HasObjectResult handler) { handler(false); }
        public override void GetObject(Hash hash, GetObjectResult handler) { handler(null); }
        public override void PutObject(BurrowObject serializedObject, PrivateIdentity identity, PutObjectResult handler) { handler(null); }
    }
}

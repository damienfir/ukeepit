using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.NullStore
{
    public class ObjectStore : Abstract.ObjectStore
    {
        public ObjectStore(string url) : base(url) { }

        public override void HasObject(Hash hash, HasObjectResult handler) { handler(false); }
        public override void GetObject(Hash hash, GetObjectResult handler) { handler(null); }
        public override void PutObject(Object serializedObject, UnlockedPrivateIdentity identity, PutObjectResult handler) { handler(null); }
    }
}

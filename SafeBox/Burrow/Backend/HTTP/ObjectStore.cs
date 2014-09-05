using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.HTTP
{
    class ObjectStore : Backend.ObjectStore
    {
        public static ObjectStore ForUrl(string url) { return null; }

        public ObjectStore(string url) : base(url, 50) { }

        public override void HasObject(Hash hash, HasObjectResult handler)
        {
            throw new NotImplementedException();
        }

        public override void GetObject(Hash hash, GetObjectResult handler)
        {
            throw new NotImplementedException();
        }

        public override void PutObject(Object serializedObject, PrivateIdentity identity, PutObjectResult handler)
        {
            throw new NotImplementedException();
        }
    }
}

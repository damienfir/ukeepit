using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.HTTP
{
    class ObjectStore : Abstract.ObjectStore
    {
        public ObjectStore(string url)
            : base(url)
        { }

        public override void HasObject(Hash hash, HasObjectResult handler)
        {
            throw new NotImplementedException();
        }

        public override void GetObject(Hash hash, GetObjectResult handler)
        {
            throw new NotImplementedException();
        }

        public override void PutObject(Object serializedObject, UnlockedPrivateIdentity identity, PutObjectResult handler)
        {
            throw new NotImplementedException();
        }
    }
}

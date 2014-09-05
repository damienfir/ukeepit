using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.HTTP
{
    class Root : Backend.Root
    {
        public Root(Account account, string name) : base(account, name) { }

        public override void List(ListResult handler)
        {
            throw new NotImplementedException();
        }

        public override void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity, PostResult handler)
        {
            throw new NotImplementedException();
        }
    }
}

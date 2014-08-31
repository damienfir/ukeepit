using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.HTTP
{
    class Root : Abstract.Root
    {
        public Root(Account account, string name) : base(account, name) { }

        public override void List(ListResult handler)
        {
            throw new NotImplementedException();
        }

        public override void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, UnlockedPrivateIdentity identity, PostResult handler)
        {
            throw new NotImplementedException();
        }
    }
}

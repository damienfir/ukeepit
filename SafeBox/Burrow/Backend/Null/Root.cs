using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.Null
{
    public class Root : Backend.Root
    {

        public Root(Account account, string name) : base(account, name) { }

        public override void List(ListResult handler) { handler(new List<ObjectUrl>()); }
        public override void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity, PostResult handler) { handler(false); }
    }
}

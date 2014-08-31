using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.NullStore
{
    public class Root : Abstract.Root
    {
        
        public Root(Account account, string name)            : base(account, name) { }

        public override void List(ListResult handler) { handler(new List<ObjectUrl>()); }
        public override void Post(IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, UnlockedPrivateIdentity identity, PostResult handler) { handler(false); }
    }
}

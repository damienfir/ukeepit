using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.HTTP
{
    class Account : Abstract.Account
    {
        public Account(ObjectStore store, Hash identityHash) : base(store, identityHash) { }

        public override Abstract.Root Root(string name)
        {
            throw new NotImplementedException();
        }

        public override void Delete(DeleteResult handler)
        {
            throw new NotImplementedException();
        }
    }
}

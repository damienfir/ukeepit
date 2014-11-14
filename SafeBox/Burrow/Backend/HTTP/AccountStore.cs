using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.HTTP
{
    class AccountStore : Backend.AccountStore
    {
        public static AccountStore ForUrl(string url) { return null; }

        public AccountStore(string url) : base(url, 50) { }

        public override void Accounts(Dictionary<string, string> query)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(Hash identityHash)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel)
        {
            throw new NotImplementedException();
        }

        public override void Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity)
        {
            throw new NotImplementedException();
        }
    }
}

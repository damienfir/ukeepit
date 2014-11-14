using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SafeBox.Burrow.Configuration;

namespace SafeBox.Burrow.Backend.Null
{
    public class AccountStore : Backend.AccountStore
    {
        public AccountStore() : base("", 100) { }

        public override IEnumerable<Hash> Accounts(Dictionary<string, string> query) { return new ImmutableStack<Hash>(); }

        public override bool Delete() { return true; }

        public override IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel) { return new ImmutableStack<ObjectUrl>(); }
        public override bool Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, PrivateIdentity identity) { return false; }

    }
}

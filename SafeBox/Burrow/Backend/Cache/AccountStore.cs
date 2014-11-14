using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow.Backend.Cache
{
    class AccountStore: Backend.AccountStore
    {
        public readonly Backend.AccountStore Cache;
        public readonly Backend.AccountStore Backend;

        public AccountStore(Backend.AccountStore cache, Backend.AccountStore backend)
            : base("(\nCache=" + cache.Url + "\nBackend=" + backend.Url + "\n)\n", backend.Priority)
        {
            this.Cache = cache;
            this.Backend = backend;
        }

        public override IEnumerable<Hash> Accounts(Dictionary<string, string> query)
        {
            var accounts = new ImmutableStack<Hash>();
            accounts = accounts.With(Cache.Accounts(query));
            accounts = accounts.With(Backend.Accounts(query));
            return accounts;
        }

        public override bool Delete(Hash identityHash)
        {
            var cacheDeleted = Cache.Delete(identityHash);
            var backendDeleted =Backend.Delete(identityHash);
            return cacheDeleted && backendDeleted;
        }

        public override IEnumerable<ObjectUrl> List(Hash identityHash, string rootLabel)
        {
            var objectUrls = new ImmutableStack<ObjectUrl>();
            objectUrls = objectUrls.With(Cache.List(identityHash, rootLabel));
            objectUrls = objectUrls.With(Backend.List(identityHash, rootLabel));
            return objectUrls;
        }

        public override bool Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, Configuration.PrivateIdentity identity)
        {
            var cacheModified = Cache.Modify(identityHash,  rootLabel,  add,  remove,  identity);
            var backendModified = Backend.Modify(identityHash, rootLabel, add, remove, identity);
            return backendModified;
        }

        // *** Asynchronous versions ***

        public override TaskGroup.Future<IEnumerable<ObjectUrl>> List(Hash identityHash, string rootLabel, TaskGroup taskGroup)
        {
            var group = new TaskGroup();
            var cacheFuture = Cache.List(identityHash, rootLabel, group);
            var backendFuture = Backend.List(identityHash, rootLabel, group);

            var future = taskGroup.WaitForMe<IEnumerable<ObjectUrl>>();
            group.WhenDone(() => {
                var list = new ImmutableStack<ObjectUrl>();
                list = list.With(cacheFuture.Result);
                list = list.With(backendFuture.Result);
                future.Done(list);
            });

            return future;
        }

        public override TaskGroup.Future<bool> Modify(Hash identityHash, string rootLabel, IEnumerable<ObjectUrl> add, IEnumerable<Hash> remove, Configuration.PrivateIdentity identity, TaskGroup taskGroup)
        {
            var group = new TaskGroup();
            var cacheFuture = Cache.Modify(identityHash, rootLabel, add, remove, identity, group);
            var backendFuture = Backend.Modify(identityHash, rootLabel, add, remove, identity, group);
            var future = taskGroup.WaitForMe<bool>();
            group.WhenDone(() => future.Done(backendFuture.Result));
            return future;
        }
    }
}

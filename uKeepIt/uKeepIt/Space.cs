using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class Space
    {
        //public readonly string title;
        public readonly string name;
        public readonly string folder;

        public Space(string name, string folder)
        {
            this.name = name;
            this.folder = folder;
        }

        public SpaceEditor CreateEditor(ArraySegment<byte> readkey, ArraySegment<byte> writekey, MultiObjectStore multiobjectstore, ImmutableStack<Store> stores)
        {
            var roots = new ImmutableStack<Root>();
            foreach (var store in stores)
                roots = roots.With(store.SpaceRoot(name));

            return new SpaceEditor(multiobjectstore, roots, readkey, writekey);
        }

        public static string encode(string name, string folder)
        {
            var cat = name + "@" + folder;
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cat));
        }

        public static Tuple<string,string> decode(string hash)
        {
            var cat = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(hash));
            var split = cat.Split('@');
            return new Tuple<string,string>(split[0], String.Join("@", split.Skip(1)));
        }
    }
}
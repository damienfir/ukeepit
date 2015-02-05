﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uKeepIt.MiniBurrow;
using uKeepIt.MiniBurrow.Folder;

namespace uKeepIt
{
    public class Space
    {
        public readonly string name;
        public readonly string folder;

        public Space(string name, string folder)
        {
            this.name = name;
            this.folder = folder;
        }

        public SpaceEditor CreateEditor(ArraySegment<byte> key, MultiObjectStore multiobjectstore, List<Store> stores)
        {
            var roots = new ImmutableStack<Root>();
            foreach (var store in stores)
                roots = roots.With(store.SpaceRoot(name));

            return new SpaceEditor(multiobjectstore, roots, key);
        }

    }
}

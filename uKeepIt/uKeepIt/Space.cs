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
        public readonly ConfigurationSnapshot Configuration;
        public readonly string Name;
        public readonly ImmutableStack<Root> Roots;
        public ArraySegment<byte> Key;

        public Space(ConfigurationSnapshot configuration, string name)
        {
            this.Name = name;

            var roots = new ImmutableStack<Root>();
            foreach (var store in configuration.Stores)
                roots = roots.With(store.SpaceRoot(name));
            this.Roots = roots;
        }

        public SpaceEditor CreateEditor(ArraySegment<byte> key)
        {
            return new SpaceEditor(Configuration, Roots, Key);
        }

    }
}
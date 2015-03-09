using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace uKeepIt
{
    public class StoreItem
    {
        public StoreItem() { }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class SpaceItem
    {
        public SpaceItem() { }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class StoreCollection : ObservableCollection<StoreItem> { public StoreCollection() { } }
    public class SpaceCollection : ObservableCollection<SpaceItem> { public SpaceCollection() { } }
}

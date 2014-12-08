using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    class CreatedFolderEntry
    {
        public readonly string Path;
        public bool Processed;

        public CreatedFolderEntry(string path)
        {
            this.Path = path;
        }
    }
}

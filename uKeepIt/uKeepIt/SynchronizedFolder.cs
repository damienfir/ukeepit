using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    class SynchronizedFolder
    {
        public readonly Space Space;
        public readonly string Folder;
        public readonly SynchronizedFolderState CreatedState;

        public SynchronizedFolder(Space space, string folder, SynchronizedFolderState createdState)
        {
            this.Space = space;
            this.Folder = folder;
            this.CreatedState = createdState;
        }


    }
}

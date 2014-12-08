using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    class Configuration
    {
        public readonly string Folder;
        public ConfigurationSnapshot Current;  // for the asynchronous thread

        public Configuration()
        {
            // Determine the configuration folder
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Folder = appDataFolder + "\\ukeep.it";

            // Load the initial configuration
            Reload();
        }

        public void Reload() {
            Current = new ConfigurationSnapshot(Folder);
        }
    }
}

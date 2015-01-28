using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    public class Configuration
    {

        public readonly string Folder;
        public ConfigurationSnapshot Current;  // for the asynchronous thread

        public delegate void ReloadedHandler(object sender, EventArgs e);
        public event ReloadedHandler Reloaded;

        public Configuration()
        {
            // Determine the configuration folder
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Folder = appDataFolder + "\\ukeepit";

            // Load the initial configuration
            Reload();
        }

        public void Reload() {
            Current = new ConfigurationSnapshot(Folder);
        }

        public MiniBurrow.Serialization.IniFile ReadConfiguration()
        {
            return Current.iniFile;
        }

        internal void WriteConfiguration(MiniBurrow.Serialization.IniFile configuration)
        {
            // write to folder
        }
    }
}

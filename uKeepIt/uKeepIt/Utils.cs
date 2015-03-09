using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uKeepIt
{
    public static class Utils
    {
        public static string choose_folder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return "";
            return dialog.SelectedPath;
        }

        public static bool checkPath(string requestedPath)
        {
            return !requestedPath.Equals("");
            // check writable, etc ...
        }
    }
}

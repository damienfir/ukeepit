using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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

        public static bool confirm_action(string message)
        {
            var button = MessageBoxButton.OKCancel;
            var result = MessageBox.Show(message, "", button);
            return result == MessageBoxResult.OK;
        }

        internal static bool checkPassword(string pw1, string pw2)
        {
            if (pw1.Equals(""))
            {
                var res = MessageBox.Show("Sorry, the password is empty", "", MessageBoxButton.OK);
                return false;
            }

            if (pw1 != pw2)
            {
                var res = MessageBox.Show("Sorry, the passwords don't match", "", MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        public static void alert(string msg)
        {
            MessageBox.Show(msg, "", MessageBoxButton.OK);
        }
    }
}

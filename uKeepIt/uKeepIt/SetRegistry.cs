using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace uKeepIt
{
    /// <summary>
    /// This class handles the interaction with the Registry to set right click handlers to work with files.
    /// Requires UAC elevated privileges.
    /// </summary>
    public static class SetRegistry
    {
        public static readonly string SetRegistryCommandLineArg = "-ConfigRegistryRightClickHandler";
        public static readonly string CommandName = "Search and Replace";

        public static bool CheckCommandLineArgs(string[] args)
        {
            bool registryProcessed = false;
            if (args != null && args.Length == 2 && args[0].Equals(SetRegistryCommandLineArg))
            {
                try
                {
                    System.IO.File.AppendAllText("COMMANDLINE_JSearchAndReplace.txt", string.Format("{0} : {1}\n", DateTime.Now.ToString(), string.Join(" ", args)));
                }
                finally
                {
                    registryProcessed = true;
                }
            }

            return registryProcessed;
        }

        public static bool StartProcessElevatedPrivileges()
        {
            // Launch itself as administrator
            ProcessStartInfo process = new ProcessStartInfo();
            process.UseShellExecute = true;
            process.WorkingDirectory = Environment.CurrentDirectory;
            process.FileName = Application.ExecutablePath;
            process.Verb = "runas";

            try
            {
                Process newProcess = Process.Start(process);
                //newProcess.WaitForExit();
            }
            catch
            {
                // The user refused to allow privileges elevation.
                // Do nothing and return directly ...
                return false;
            }

            return true;
        }

        public static bool register()
        {
            try
            {
                var directoryKey = Registry.ClassesRoot.OpenSubKey("Directory");
                var shellKey = directoryKey.OpenSubKey("shell", true);

                var regKey = shellKey.OpenSubKey("ukeepit", true);
                if (regKey == null) {
                    regKey = shellKey.CreateSubKey("ukeepit");
                    regKey.Close();
                    regKey = shellKey.OpenSubKey("ukeepit", true);
                }
                    
                var commandKey = regKey.OpenSubKey("command", true);
                if (commandKey == null) {
                    commandKey = regKey.CreateSubKey("command");
                    commandKey.Close();
                    commandKey = regKey.OpenSubKey("command");
                }

                var commandValue = commandKey.GetValue(null);
                if (commandValue == null || (commandValue.GetType() == typeof(string) && string.IsNullOrEmpty((string)commandValue)))
                {
                    commandKey.SetValue(null, string.Format("\"{0}\" \"%L\"", Application.ExecutablePath), RegistryValueKind.ExpandString);
                    return true;
                }
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                //// Rollback entries that were added to the registry (LIFO)
                //try
                //{
                //    for (int i = regKeyCreatedList.Count - 1; i >= 0; i--)
                //    {
                //        RegistryKey regKey = regKeyCreatedList[i];
                //        regKey.DeleteSubKeyTree("");  // TODO: verify this works
                //    }
                //}
                //catch (Exception ex2)
                //{
                //    error += "\n\nIn addition, there was the following error trying to rollback:\n" + ex2.Message;
                //}

                if (!StartProcessElevatedPrivileges())
                {
                    MessageBox.Show(ex.Message, "Not enough permissions to update the registry");
                }
                return false;
            }
        }
    }
}
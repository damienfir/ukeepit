using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            App.Configuration.Reloaded += BurrowConfiguration_Reloaded;
            BurrowConfiguration_Reloaded(null, null);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            AddFolder(dialog.SelectedPath);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void show_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as MenuItem;
            var folder = btn.Tag as string;
            try { System.Diagnostics.Process.Start(folder); }
            catch (Exception) { MessageBox.Show("Unable to open the folder (with Windows Explorer). Does the folder exist?"); }
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as MenuItem;
            var item = btn.Tag as string;
            // TODO: here, we should just mark the store for removal, and then copy all objects and our accounts to a remaining permanent store
            //Static.FileDelete(item.StoreFile, LogLevel.Warning);
            App.Configuration.Reload();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // If we got files, add them to the list
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    AddFolder(file);
            }
        }

        static Regex burrowBandFolder = new Regex(@"^(.*)\\\d{8,8}T\d{6,6}Z$");

        private void AddFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;

            // Check if there is a Burrow store somewhere around this folder
            folder = BurrowStoreFolder(folder);

            // Check the folder length (this is a current limitation)
            if (folder.Length > 80)
            {
                MessageBox.Show("The path '" + folder + "' is too long to be used as ukeep!t folder.");
                return;
            }

            // Create the objects and accounts directories
            MiniBurrow.Static.DirectoryCreate(folder + "\\objects");
            MiniBurrow.Static.DirectoryCreate(folder + "\\accounts");

            // Add this store to the configuration file
            var configuration = App.Configuration.ReadConfiguration();
            var section = configuration.Section("store "+ MiniBurrow.Static.RandomHex(8));
            section.Set("url", folder);
            section.Set("permanent", true);
            App.Configuration.WriteConfiguration(configuration);

            // Reload the configuration
            App.Configuration.Reload();
        }

        private string BurrowStoreFolder(string folder)
        {
            // If this is a Burrow store, use it without modification
            if (Directory.Exists(folder + "\\objects") && Directory.Exists(folder + "\\accounts")) return folder;

            // Check if we are inside a Burrow store
            var candidate = folder;
            while (true)
            {
                var name = System.IO.Path.GetFileName(candidate).ToLower();
                candidate = System.IO.Path.GetDirectoryName(candidate);
                if (candidate == null) break;
                if (name == "objects" && Directory.Exists(candidate + "\\accounts") || name == "accounts" && Directory.Exists(candidate + "\\objects")) return candidate;
            }

            // Otherwise, add "ukeep.it"
            return folder + "\\ukeep.it";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Configuration.Reloaded -= BurrowConfiguration_Reloaded;
        }

        void BurrowConfiguration_Reloaded(object sender, EventArgs e)
        {
            // Update the list
            stores.Children.Clear();
            var configuration = App.Configuration.ReadConfiguration();
            foreach (var pairs in configuration.SectionsByName)
            {
                if (! pairs.Key.StartsWith("store ")) continue;
                var grid = new Grid();
                grid.Tag = pairs.Key;
                grid.Margin = new Thickness(0, 10, 0, 10);
                stores.Children.Add(grid);
                //var icon = new Image();
                //icon.Source = ;
                var label = new Label();
                var url = pairs.Value.Get("url");
                label.Content = url;
                label.Foreground = url.StartsWith("http://") || url.StartsWith("https://") || Directory.Exists(url) ? Brushes.Black : Brushes.Red;
                grid.Children.Add(label);
            }
        }
    }
}

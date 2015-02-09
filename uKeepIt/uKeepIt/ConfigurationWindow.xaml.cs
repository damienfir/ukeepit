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
        private Configuration _config;
        public ConfigurationWindow(Configuration config)
        {
            this._config = config;

            InitializeComponent();
            //App.Configuration.Reloaded += BurrowConfiguration_Reloaded;
            //BurrowConfiguration_Reloaded(null, null);
            reloadWindow();
        }

        private void window_drop(object sender, DragEventArgs e)
        {
            // If we got files, add them to the list
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    add_space(file);
            }
        }

        private void reloadWindow()
        {
            draw_stores();
            draw_spaces();
        }

        private void draw_stores()
        {
            cloud_stack.Children.Clear();
            foreach (var store in _config.stores)
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                var label1 = new Label();
                label1.Content = store.Key;
                Grid.SetColumn(label1, 0);
                grid.Children.Add(label1);

                var label2 = new Label();
                label2.Content = store.Value.Folder;
                Grid.SetColumn(label2, 1);
                grid.Children.Add(label2);

                var delbutton = new Button();
                delbutton.Content = "delete";
                delbutton.Tag = store.Key;
                delbutton.Click += store_del_button_click;
                Grid.SetColumn(delbutton, 2);
                grid.Children.Add(delbutton);

                cloud_stack.Children.Add(grid);
            }
        }

        private void draw_spaces()
        {
            folders_stack.Children.Clear();
            foreach (var space in _config.spaces)
            {
                var label = new Label();
                label.Content = string.Format("{0} ({1})", space.Key, space.Value.folder);
                folders_stack.Children.Add(label);
            }
        }

        bool confirmAction(string message)
        {
            var button = MessageBoxButton.OKCancel;
            var result = MessageBox.Show(message, "", button);
            return result == MessageBoxResult.OK;
        }

        void execute(bool success)
        {
            if (success)
            {
                _config.reloadContext();
                _config.writeConfig();
            }
        }

        private void show_click(object sender, RoutedEventArgs e)
        {
            var btn = sender as MenuItem;
            var folder = btn.Tag as string;
            try { System.Diagnostics.Process.Start(folder); }
            catch (Exception) { MessageBox.Show("Unable to open the folder (with Windows Explorer). Does the folder exist?"); }
        }

        private void password_button_click(object sender, RoutedEventArgs e)
        {

        }

        private void store_add_button_click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            add_store(dialog.SelectedPath);
            draw_stores();
        }

        void store_del_button_click(object sender, RoutedEventArgs e)
        {
            var to_remove = ((Button)sender).Tag;

            if (!confirmAction("remove store"))
                return;

            remove_store(to_remove as String);
            draw_stores();
        }

        private void space_add_button_click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            add_space(dialog.SelectedPath);
            draw_spaces();
        }

        private void space_del_button_click(object sender, RoutedEventArgs e)
        {

        }

        private void done_button_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void add_store(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            folder += @"\ukeepit";

            execute(_config.addStore(name, folder));
        }

        private void remove_store(string name)
        {
            execute(_config.removeStore(name));
        }

        private void add_space(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            execute(_config.addSpace(name, folder));
        }

        private void remove_space(string name)
        {
            execute(_config.removeSpace(name));
        }
    }
}
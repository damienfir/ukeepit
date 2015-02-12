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

        public void reloadWindow()
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

                var delete_btn = new Button();
                delete_btn.Content = "delete";
                delete_btn.Tag = store.Key;
                delete_btn.Click += store_del_button_click;
                Grid.SetColumn(delete_btn, 2);
                grid.Children.Add(delete_btn);

                cloud_stack.Children.Add(grid);
            }
        }

        private void draw_spaces()
        {
            folders_stack.Children.Clear();
            foreach (var space in _config.spaces)
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                var label = new Label();
                label.Content = space.Key;
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var label2 = new Label();
                label2.Content = space.Value.folder;
                Grid.SetColumn(label2, 1);
                grid.Children.Add(label2);

                if (space.Value.folder != _config._default_folder)
                {
                    var remove_btn = new Button();
                    remove_btn.Content = "remove locally";
                    remove_btn.Tag = space.Key;
                    remove_btn.Click += space_remove_button_click;
                    Grid.SetColumn(remove_btn, 2);
                    grid.Children.Add(remove_btn);
                }
                else
                {
                    var checkout_btn = new Button();
                    checkout_btn.Content = "checkout";
                    checkout_btn.Tag = space.Key;
                    checkout_btn.Click += space_checkout_button_click;
                    Grid.SetColumn(checkout_btn, 2);
                    grid.Children.Add(checkout_btn);

                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                    var delete_btn = new Button();
                    delete_btn.Content = "delete permanently";
                    delete_btn.Tag = space.Key;
                    delete_btn.Click += space_delete_button_click;
                    Grid.SetColumn(delete_btn, 3);
                    grid.Children.Add(delete_btn);
                }

                folders_stack.Children.Add(grid);
            }
        }

        bool confirm_action(string message)
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

        private string choose_folder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return "";
            return dialog.SelectedPath;
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
            add_store(choose_folder());
            draw_stores();
        }

        void store_del_button_click(object sender, RoutedEventArgs e)
        {
            if (confirm_action("remove store"))
            {
                var to_remove = ((Button)sender).Tag as string;
                remove_store(to_remove as String);
                draw_stores();
            }
        }

        private void space_add_button_click(object sender, RoutedEventArgs e)
        {
            var target = choose_folder();
            if (target != "")
                add_space(target);
            draw_spaces();
        }

        private void space_remove_button_click(object sender, RoutedEventArgs e)
        {
            var to_remove = ((Button)sender).Tag as string;
            _config.removeSpace(to_remove);
            execute(_config.addSpace(to_remove, _config._default_folder));
            draw_spaces();
        }

        private void space_checkout_button_click(object sender, RoutedEventArgs e)
        {
            var to_checkout = (sender as Button).Tag as string;
            var target_location = choose_folder();

            checkout_space(to_checkout, target_location);
        }

        private void space_delete_button_click(object sender, RoutedEventArgs e)
        {
            if (confirm_action("remove space permanently ?"))
            {
                var to_remove = ((Button)sender).Tag as string;
                execute(_config.removeSpace(to_remove, true));
                draw_spaces();
            }
        }

        private void done_button_click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void add_store(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            folder += @"\ukeepit";

            execute(_config.addStore(name, folder));
            draw_spaces();
        }

        private void remove_store(string name)
        {
            execute(_config.removeStore(name));
            draw_stores();
        }

        private void add_space(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            execute(_config.addSpace(name, folder));
            draw_spaces();
        }

        private void remove_space(string name)
        {
            execute(_config.removeSpace(name));
            draw_spaces();
        }

        private void checkout_space(string name, string target_location)
        {
            _config.removeSpace(name);
            execute(_config.addSpace(name, target_location));
            draw_spaces();
        }
    }
}
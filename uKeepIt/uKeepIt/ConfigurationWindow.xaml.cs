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
using System.Collections.ObjectModel;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private Configuration _config;
        private TextBox pw_input1;
        private TextBox pw_input2;
        private TextBox pw_input_check;

        private ObservableCollection<StoreItem> store_items;
        private ObservableCollection<SpaceItem> space_items;

        public ConfigurationWindow(Configuration config)
        {
            this._config = config;

            InitializeComponent();
            //App.Configuration.Reloaded += BurrowConfiguration_Reloaded;
            //BurrowConfiguration_Reloaded(null, null);
            //reloadWindow();

            store_items = new ObservableCollection<StoreItem>();
            space_items = new ObservableCollection<SpaceItem>();

            loadStores();
            loadSpaces();
            StoreView.ItemsSource = store_items;
            SpaceView.ItemsSource = space_items;
        }

        //private void draw_password(bool check = false)
        //{
        //    var name = "pwgrid";
        //    Grid grid = (Grid)maingrid.FindName(name);
        //    if (grid == null)
        //    {
        //        grid = new Grid();
        //        grid.Name = name;
        //        maingrid.RegisterName(name, grid);
        //        Grid.SetColumn(grid, 1);
        //        maingrid.Children.Add(grid);
        //    }
        //    else
        //    {
        //        grid.Children.Clear();
        //        grid.ColumnDefinitions.Clear();
        //        grid.RowDefinitions.Clear();
        //    }

        //    if (check)
        //    {
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());

        //        pw_input_check = new TextBox();
        //        Grid.SetColumn(pw_input_check, 0);
        //        grid.Children.Add(pw_input_check);

        //        var btn_check = new Button();
        //        btn_check.Content = "Verify";
        //        btn_check.Click += password_check_button_click;
        //        Grid.SetColumn(btn_check, 1);
        //        grid.Children.Add(btn_check);
        //    }
        //    else if (_config.getKey() == null)
        //    {
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.RowDefinitions.Add(new RowDefinition());
        //        grid.RowDefinitions.Add(new RowDefinition());

        //        pw_input1 = new TextBox();
        //        Grid.SetRow(pw_input1, 0);
        //        grid.Children.Add(pw_input1);

        //        pw_input2 = new TextBox();
        //        Grid.SetRow(pw_input2, 1);
        //        grid.Children.Add(pw_input2);

        //        var btn_set = new Button();
        //        btn_set.Content = "Set";
        //        btn_set.Click += password_set_button_click;
        //        Grid.SetColumn(btn_set, 1);
        //        grid.Children.Add(btn_set);
        //    }
        //    else
        //    {
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());

        //        var label = new Label();
        //        label.Content = "password already set";
        //        grid.Children.Add(label);

        //        var btn_change = new Button();
        //        btn_change.Content = "Change";
        //        btn_change.Click += password_change_button_click;
        //        Grid.SetColumn(btn_change, 1);

        //        grid.Children.Add(btn_change);
        //    }
        //}

        private void loadStores()
        {
            store_items.Clear();
            foreach (var store in _config.stores)
            {
                store_items.Add(new StoreItem() { Name = store.Key, Path = store.Value.Folder });
            }
        }

        private void loadSpaces()
        {
            space_items.Clear();
            foreach (var store in _config.spaces)
            {
                space_items.Add(new SpaceItem() { Name = store.Key, Path = store.Value.folder });
            }
        }
        //private void draw_spaces()
        //{
        //    folders_stack.Children.Clear();
        //    foreach (var space in _config.spaces)
        //    {
        //        var grid = new Grid();
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());
        //        grid.ColumnDefinitions.Add(new ColumnDefinition());

        //        var label = new Label();
        //        label.Content = space.Key;
        //        Grid.SetColumn(label, 0);
        //        grid.Children.Add(label);

        //        var label2 = new Label();
        //        label2.Content = space.Value.folder;
        //        Grid.SetColumn(label2, 1);
        //        grid.Children.Add(label2);

        //        if (space.Value.folder != _config._default_folder)
        //        {
        //            var remove_btn = new Button();
        //            remove_btn.Content = "remove locally";
        //            remove_btn.Tag = space.Key;
        //            remove_btn.Click += space_remove_button_click;
        //            Grid.SetColumn(remove_btn, 2);
        //            grid.Children.Add(remove_btn);
        //        }
        //        else
        //        {
        //            var checkout_btn = new Button();
        //            checkout_btn.Content = "checkout";
        //            checkout_btn.Tag = space.Key;
        //            checkout_btn.Click += space_checkout_button_click;
        //            Grid.SetColumn(checkout_btn, 2);
        //            grid.Children.Add(checkout_btn);

        //            grid.ColumnDefinitions.Add(new ColumnDefinition());

        //            var delete_btn = new Button();
        //            delete_btn.Content = "delete permanently";
        //            delete_btn.Tag = space.Key;
        //            delete_btn.Click += space_delete_button_click;
        //            Grid.SetColumn(delete_btn, 3);
        //            grid.Children.Add(delete_btn);
        //        }

        //        folders_stack.Children.Add(grid);
        //    }
        //}

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

        private void password_set_button_click(object sender, RoutedEventArgs e)
        {
            var pw1 = pw_input1.Text;
            var pw2 = pw_input2.Text;
            set_password(pw1, pw2);
        }

        private bool set_password(string pw1, string pw2)
        {
            if (pw1.Equals(""))
            {
                var res = MessageBox.Show("password empty", "", MessageBoxButton.OK);
                return false;
            }

            if (pw1 != pw2)
            {
                var res = MessageBox.Show("passwords don't match", "", MessageBoxButton.OK);
                return false;
            }

            _config.changeKey(pw1);
            //draw_password();
            return true;
        }

        private void password_check_button_click(object sender, RoutedEventArgs e)
        {
            var pw = pw_input_check.Text;
            if (_config.invalidateKey(pw))
            {
                //draw_password();
            }
            else
            {
                MessageBox.Show("password is not correct", "", MessageBoxButton.OK);
            }
        }

        private void password_change_button_click(object sender, RoutedEventArgs e)
        {
            //draw_password(true);
        }

        private void StoreAdd_Click(object sender, RoutedEventArgs e)
        {
            add_store(choose_folder());
            //draw_stores();
        }

        void StoreDelete_Click(object sender, RoutedEventArgs e)
        {
            if (confirm_action("remove store"))
            {
                var to_remove = ((Button)sender).Tag as string;
                remove_store(to_remove as String);
                //draw_stores();
            }
        }

        private void SpaceAdd_Click(object sender, RoutedEventArgs e)
        {
            var target = choose_folder();
            if (target != "")
                add_space(target);
            //draw_spaces();
        }

        private void space_remove_button_click(object sender, RoutedEventArgs e)
        {
            var to_remove = ((Button)sender).Tag as string;
            _config.removeSpace(to_remove);
            execute(_config.addSpace(to_remove, _config._default_folder));
            //draw_spaces();
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
                //draw_spaces();
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
            loadStores();
            //draw_spaces();
        }

        private void remove_store(string name)
        {
            execute(_config.removeStore(name));
            loadStores();
            //draw_stores();
        }

        private void add_space(string folder)
        {
            string name = folder.Replace(System.IO.Path.GetDirectoryName(folder) + "\\", "");
            execute(_config.addSpace(name, folder));
            //draw_spaces();
            loadSpaces();
        }

        private void remove_space(string name)
        {
            execute(_config.removeSpace(name));
            //draw_spaces();
        }

        private void checkout_space(string name, string target_location)
        {
            _config.removeSpace(name);
            execute(_config.addSpace(name, target_location));
            //draw_spaces();
        }

        private void quit_btn_click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    public class StoreItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class SpaceItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    //public static class StoreCommands
    //{
    //    public static readonly RoutedUICommand Delete = new RoutedUICommand("Delete Store", "Delete", typeof(StoreCommands), null);
    //}

    public static class SpaceTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var space = item as SpaceItem;

            if (!space.Path.Equals(""))
            {
                return element.FindResource("SpaceCheckedoutTemplate") as DataTemplate;
            }
            else
            {
                return element.FindResource("SpaceNotCheckedoutTemplate") as DataTemplate;
            }
        }
    }
}
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
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow
    {
        private Configuration _config;
        public delegate void ConfigWatcher();

        private ObservableCollection<StoreItem> store_items;
        private ObservableCollection<SpaceItem> space_items;

        public ConfigurationWindow(Configuration config)
        {
            this._config = config;
            _config.registerOnChanged(this);

            Closing += ConfigurationWindow_Closing;

            InitializeComponent();

            store_items = new ObservableCollection<StoreItem>();
            space_items = new ObservableCollection<SpaceItem>();

            loadStores();
            loadSpaces();

            StoreView.ItemsSource = store_items;
            SpaceView.ItemsSource = space_items;

            initializePasswordView();

            firstStart();
        }

        private void firstStart()
        {
            if (_config.key.Item2 == null)
            {
                Show();
            }
        }

        void ConfigurationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            _config.reloadContext();
        }

        public void notify()
        {
            var del = new ConfigWatcher(notifyCallback);
            Application.Current.Dispatcher.BeginInvoke(del, null);
        }

        public void notifyCallback()
        {
            loadStores();
            loadSpaces();
        }

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

        private void SetPassword_Click(object sender, RoutedEventArgs e)
        {
            var pw1 = (uiPasswordWrapper.Template.FindName("password_input1", uiPasswordWrapper) as TextBox).Text;
            var pw2 = (uiPasswordWrapper.Template.FindName("password_input2", uiPasswordWrapper) as TextBox).Text;
            setPassword(pw1, pw2);
        }

        private void initializePasswordView()
        {
            if (_config.key.Item2 == null)
            {
                changePasswordTemplate("uiPasswordNotSetTemplate", false);
            }
            else
            {
                changePasswordTemplate("uiPasswordSetTemplate");
            }
        }

        private bool setPassword(string pw1, string pw2)
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

            execute(_config.changeKey(pw1));
            initializePasswordView();
            return true;
        }

        private void VerifyPassword_Click(object sender, RoutedEventArgs e)
        {
            var pw = (uiPasswordWrapper.Template.FindName("password_input_verify", uiPasswordWrapper) as TextBox).Text;
            if (_config.invalidateKey(pw))
            {
                changePasswordTemplate("uiPasswordNotSetTemplate");
            }
            else
            {
                MessageBox.Show("password is not correct", "", MessageBoxButton.OK);
            }
        }

        private void changePasswordTemplate(string template, bool withCancel = true)
        {
            uiPasswordWrapper.Template = uiPasswordWrapper.FindResource(template) as ControlTemplate;
            if (!withCancel)
            {
                Button cancelButton = uiPasswordWrapper.FindName("CancelPasswordButton") as Button;
                if (cancelButton != null) (cancelButton.Parent as UniformGrid).Children.Remove(cancelButton);
            }
        }

        private void CancelPassword_Click(object sender, RoutedEventArgs e)
        {
            initializePasswordView();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            changePasswordTemplate("uiPasswordVerifyTemplate");
        }

        private void StoreAdd_Click(object sender, RoutedEventArgs e)
        {
            string requestedPath = choose_folder();
            if (checkPath(requestedPath))
            {
                add_store(requestedPath);
                loadStores();
            }
        }

        private bool checkPath(string requestedPath)
        {
            return !requestedPath.Equals("");
            // check writable, etc ...
        }

        void StoreDelete_Click(object sender, RoutedEventArgs e)
        {
            if (confirm_action("remove store"))
            {
                var to_remove = ((Button)sender).Tag as string;
                remove_store(to_remove as String);
                loadStores();
            }
        }

        private void SpaceShow_Click(object sender, RoutedEventArgs e)
        {
            var toShow = SpaceView.SelectedItem as SpaceItem;
            if (toShow != null)
            {
                try
                {
                    Process.Start(toShow.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void SpaceAdd_Click(object sender, RoutedEventArgs e)
        {
            var target = choose_folder();
            if (target != "")
            {
                add_space(target);
                loadSpaces();
            }
        }

        private void SpaceRemove_Click(object sender, RoutedEventArgs e)
        {
            var to_remove = SpaceView.SelectedItem as SpaceItem;
            if (to_remove.Path.Equals(""))
            {
                if (confirm_action("remove space permanently ?"))
                {
                    execute(_config.deleteSpace(to_remove.Name));
                    loadSpaces();
                }
            }
            else
            {
                if (confirm_action("remove space locally ?"))
                {
                    _config.removeSpace(to_remove.Name);
                    execute(_config.addSpace(to_remove.Name, _config._default_folder));
                }
            }
            loadSpaces();
        }

        private void SpaceCheckout_Click(object sender, RoutedEventArgs e)
        {
            var to_checkout = (sender as Button).Tag as string;
            var target_location = choose_folder();

            checkout_space(to_checkout, target_location);
            loadSpaces();
        }

        private void done_button_click(object sender, RoutedEventArgs e)
        {
            Hide();
            _config.reloadContext();
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
            execute(_config.addSpace(folder));
        }

        private void remove_space(string name)
        {
            execute(_config.removeSpace(name));
        }

        private void checkout_space(string name, string target_location)
        {
            _config.removeSpace(name);
            execute(_config.addSpace(name, target_location));
        }

        private void quit_btn_click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    public class StoreItem
    {
        public StoreItem() { }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class SpaceItem
    {
        public SpaceItem() { }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class StoreCollection : ObservableCollection<StoreItem> { public StoreCollection() { } }
    public class SpaceCollection : ObservableCollection<SpaceItem> { public SpaceCollection() { } }

    public class SpaceTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var space = item as SpaceItem;

            if (!space.Path.Equals("") && !space.Path.Equals("--"))
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
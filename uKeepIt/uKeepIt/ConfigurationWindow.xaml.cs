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

        private StoreCollection store_items;
        private SpaceCollection space_items;

        public ConfigurationWindow(Configuration config)
        {
            this._config = config;
            _config.registerOnChanged(this);

            Closing += ConfigurationWindow_Closing;
            Activated += ConfigurationWindow_Activated;

            InitializeComponent();

            store_items = new StoreCollection();
            space_items = new SpaceCollection();
            StoreView.ItemsSource = store_items;
            SpaceView.ItemsSource = space_items;

            reloadUI();
        }

        private void ConfigurationWindow_Activated(object sender, EventArgs e)
        {
            reloadUI();
        }

        public void reloadUI()
        {
            //_config.readConfig();
            loadStores();
            loadSpaces();
            initializePasswordView();
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
            if (Utils.checkPassword(pw1, pw2))
            {
                _config.editor.change_key(pw1);
                initializePasswordView();
                return true;
            }
            return false;
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
                Utils.alert("Sorry, the password is not correct");
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
            string requestedPath = Utils.choose_folder();
            if (Utils.checkPath(requestedPath))
            {
                _config.editor.add_store(requestedPath);
                loadStores();
            }
        }

        void StoreDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.confirm_action("remove store"))
            {
                var to_remove = ((Button)sender).Tag as string;
                _config.editor.remove_store_with_files(to_remove as String);
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
            var target = Utils.choose_folder();
            if (target != "")
            {
                _config.editor.add_space(target);
                loadSpaces();
            }
        }

        private void SpaceRemove_Click(object sender, RoutedEventArgs e)
        {
            var to_remove = SpaceView.SelectedItem as SpaceItem;
            if (to_remove == null) return;

            if (to_remove.Path.Equals(""))
            {
                if (Utils.confirm_action("remove space permanently ?"))
                {
                    _config.editor.delete_space(to_remove.Name);
                    loadSpaces();
                }
            }
            else
            {
                if (Utils.confirm_action("remove space locally ?"))
                {
                    _config.editor.checkout_space(to_remove.Name, _config._default_folder);
                    //_config.removeSpace(to_remove.Name);
                    //_config.editor.execute(_config.addSpace(to_remove.Name, _config._default_folder));
                }
            }
            loadSpaces();
        }

        private void SpaceCheckout_Click(object sender, RoutedEventArgs e)
        {
            var to_checkout = (sender as Button).Tag as string;
            var target_location = Utils.choose_folder();

            _config.editor.checkout_space(to_checkout, target_location);
            loadSpaces();
        }
        
        private void done_button_click(object sender, RoutedEventArgs e)
        {
            Hide();
            _config.reloadContext();
        }

        private void quit_btn_click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

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
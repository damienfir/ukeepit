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
using System.Windows.Shapes;

namespace uKeepIt
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WelcomeWindow
    {
        Configuration _config;
        private StoreCollection store_items;

        public WelcomeWindow(Configuration config)
        {
            _config = config;

            store_items = new StoreCollection();
            loadStores();
            StoreView.ItemsSource = store_items;

            InitializeComponent();
            Show();
        }

        private void loadStores()
        {
            store_items.Clear();
            foreach (var store in _config.stores)
            {
                store_items.Add(new StoreItem() { Name = store.Key, Path = store.Value.Folder });
            }
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

        private void StoreDelete_Click(object sender, RoutedEventArgs e)
        {
            var to_remove = ((Button)sender).Tag as string;
            _config.editor.remove_store(to_remove as String);
        }

        private void SetPassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloudsContinue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

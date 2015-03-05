using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace uKeepIt
{
    class NotificationMenu
    {
        ConfigurationWindow _configwindow;
        NotifyIcon _icon;

        public NotificationMenu(ConfigurationWindow configwindow)
        {
            this._configwindow = configwindow;

            ContextMenuStrip menu = new ContextMenuStrip();
            
            menu.Items.Add(new ToolStripButton("Configuration", null, onConfiguration));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripButton("Quit", null, onQuit));

            _icon = new NotifyIcon(new System.ComponentModel.Container())
            {
                ContextMenuStrip = menu,
                Icon = new Icon(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ukeepit\\ukeepit-tray.ico"),
                Text = "ukeepit",
                Visible = true
            };

            _icon.Click += icon_Click;

            //showConfig();
        }

        void icon_Click(object sender, EventArgs e)
        {
            MouseEventArgs a = e as MouseEventArgs;
            if (a.Button != MouseButtons.Right)
                showConfig();
        }

        private void onConfiguration(object sender, EventArgs e)
        {
            showConfig();
        }

        private void onQuit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void showConfig()
        {
            try
            {
                _configwindow.Show();
                //_configwindow.reloadWindow();
            }
            catch (InvalidOperationException) { }
        }
    }
}
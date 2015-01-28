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
        App app;
        NotifyIcon notifyicon;
        ContextMenuStrip menu;

        public NotificationMenu(App app)
        {
            this.app = app;

            menu = new ContextMenuStrip();
            
            menu.Items.Add(new ToolStripButton("Configuration", null, onConfiguration));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripButton("Quit", null, onQuit));

            notifyicon = new NotifyIcon(new System.ComponentModel.Container())
            {
                ContextMenuStrip = menu,
                Icon = new Icon("icon.ico"),
                Text = "ukeepit",
                Visible = true
            };

        }

        private void onConfiguration(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void onQuit(object sender, EventArgs e)
        {
            app.Shutdown();
        }
    }
}
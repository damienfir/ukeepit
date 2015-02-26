﻿#pragma checksum "..\..\..\ConfigurationWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6D866C2AE0EA96AB7EAEFB5ED1A740F4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using uKeepIt;


namespace uKeepIt {
    
    
    /// <summary>
    /// ConfigurationWindow
    /// </summary>
    public partial class ConfigurationWindow : MahApps.Metro.Controls.MetroWindow, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 44 "..\..\..\ConfigurationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Control uiPasswordWrapper;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\ConfigurationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl StoreView;
        
        #line default
        #line hidden
        
        
        #line 123 "..\..\..\ConfigurationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl SpaceView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/uKeepIt;component/configurationwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\ConfigurationWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.uiPasswordWrapper = ((System.Windows.Controls.Control)(target));
            return;
            case 7:
            this.StoreView = ((System.Windows.Controls.ItemsControl)(target));
            return;
            case 9:
            
            #line 108 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.StoreAdd_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.SpaceView = ((System.Windows.Controls.ItemsControl)(target));
            return;
            case 15:
            
            #line 188 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SpaceAdd_Click);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 201 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.quit_btn_click);
            
            #line default
            #line hidden
            return;
            case 17:
            
            #line 202 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.done_button_click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 2:
            
            #line 49 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ChangePassword_Click);
            
            #line default
            #line hidden
            break;
            case 3:
            
            #line 62 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelPassword_Click);
            
            #line default
            #line hidden
            break;
            case 4:
            
            #line 63 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SetPassword_Click);
            
            #line default
            #line hidden
            break;
            case 5:
            
            #line 74 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelPassword_Click);
            
            #line default
            #line hidden
            break;
            case 6:
            
            #line 75 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.VerifyPassword_Click);
            
            #line default
            #line hidden
            break;
            case 8:
            
            #line 94 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.StoreDelete_Click);
            
            #line default
            #line hidden
            break;
            case 11:
            
            #line 133 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SpaceCheckout_Click);
            
            #line default
            #line hidden
            break;
            case 12:
            
            #line 143 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SpaceDelete_Click);
            
            #line default
            #line hidden
            break;
            case 13:
            
            #line 164 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SpaceShow_Click);
            
            #line default
            #line hidden
            break;
            case 14:
            
            #line 174 "..\..\..\ConfigurationWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SpaceRemove_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}


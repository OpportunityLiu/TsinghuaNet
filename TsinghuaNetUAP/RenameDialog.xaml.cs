using System;
using TsinghuaNet.Web;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    public sealed partial class RenameDialog : ContentDialog
    {
        public RenameDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            textBoxRename.SelectAll();
            textBoxRename.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        public string NewName
        {
            get
            {
                return textBoxRename.Text;
            }
            set
            {
                textBoxRename.Text = value ?? "";
            }
        }
    }
}

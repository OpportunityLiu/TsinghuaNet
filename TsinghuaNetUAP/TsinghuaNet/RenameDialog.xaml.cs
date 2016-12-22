using System;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ChangeName = true;
                this.Hide();
            }
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

        public bool ChangeName
        {
            get;
            private set;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            ChangeName = false;
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if(args.Result == ContentDialogResult.Primary)
                ChangeName = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TsinghuaNet
{
    public sealed partial class WebContentHolder : UserControl
    {
        public WebContentHolder()
        {
            this.InitializeComponent();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = availableSize.Width;
            var height = availableSize.Height;
            if(width >= 1024)
            {
                this.contentPresenter.Height = height;
                this.contentPresenter.Width = width;
            }
            else
            {
                this.contentPresenter.Width = 1024;
                this.contentPresenter.Height = height / width * 1024;
            }
            return base.MeasureOverride(availableSize);
        }

        public object WebContent
        {
            get
            {
                return GetValue(WebContentProperty);
            }
            set
            {
                SetValue(WebContentProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for WebContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebContentProperty =
            DependencyProperty.Register("WebContent", typeof(object), typeof(WebContentHolder), new PropertyMetadata(null));
    }
}

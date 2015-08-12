using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Web
{
    static class DispatcherHelper
    {
        private static CoreDispatcher dispatcher = Window.Current?.Dispatcher;

        public static IAsyncAction Run(Action action)
        {
            return AsyncInfo.Run(async token =>
            {
                if(dispatcher == null)
                    action();
                else
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            });
        }
    }
}

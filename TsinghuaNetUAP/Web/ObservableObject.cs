using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Web
{
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性更改时引发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 引发 <see cref="PropertyChanged"/> 事件。
        /// </summary>
        /// <param name="propertyName">更改的属性名，默认值表示调用方名称。</param>
        protected async void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            await DispatcherHelper.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected void Set<T>(ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, newValue))
                return;
            field = newValue;
            RaisePropertyChanged(propertyName);
        }
    }
}

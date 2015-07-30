using TsinghuaNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Globalization;
using TsinghuaNet.Web;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234237 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 基本页，提供大多数应用程序通用的特性。
    /// </summary>
    public sealed partial class SingleMonthData : Page
    {
        private NavigationHelper navigationHelper;

        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get
            {
                return this.navigationHelper;
            }
        }


        public SingleMonthData()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        ///使用在导航过程中传递的内容填充页。 在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。 首次访问页面时，该状态将为 null。</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var delta = 1E-9;
            KeyValuePair<DateTime, MonthlyData> data;
            try
            {
                data = (KeyValuePair<DateTime, MonthlyData>)e.NavigationParameter;
            }
            catch(InvalidCastException ex)
            {
                throw new InvalidOperationException("无法从此参数导航。", ex);
            }
            textBlockTitle.Text = data.Key.ToString(CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern, CultureInfo.InvariantCulture);
            Func<KeyValuePair<DateTime, MonthlyData>, List<KeyValuePair<double, Size>>> loadDataPrevious = d =>
            {
                var dataContext = new List<KeyValuePair<double, Size>>();
                var sum = Size.MinValue;
                var day = d.Key;
                var dayMax = DateTime.DaysInMonth(day.Year, day.Month);
                for(double i = 1; i <= dayMax; i++)
                {
                    try
                    {
                        sum += d.Value[day];
                    }
                    catch(KeyNotFoundException)
                    {
                    }
                    dataContext.Add(new KeyValuePair<double, Size>(i - delta, sum));
                    dataContext.Add(new KeyValuePair<double, Size>(i, new Size()));
                    dataContext.Add(new KeyValuePair<double, Size>(i + delta, sum));
                    day = day.AddDays(1);
                }
                return dataContext;
            }, loadDataCurrent = d =>
            {
                var dataContext = new List<KeyValuePair<double, Size>>();
                var sum = Size.MinValue;
                var day = d.Key;
                var dayMax = DateTime.DaysInMonth(day.Year, day.Month);
                var dayNow = DateTime.Now.Day;
                var predictIncrease = d.Value.Sum / dayNow;
                double i;
                for(i = 1d; i <= dayNow; i++)
                {
                    try
                    {
                        sum += d.Value[day];
                    }
                    catch(KeyNotFoundException)
                    {
                    }
                    dataContext.Add(new KeyValuePair<double, Size>(i - delta, sum));
                    dataContext.Add(new KeyValuePair<double, Size>(i, new Size()));
                    dataContext.Add(new KeyValuePair<double, Size>(i + delta, sum));
                    day = day.AddDays(1);
                }
                dataContext.Add(new KeyValuePair<double, Size>(i - 1 + 2*delta, new Size()));
                for(; i <= dayMax; i++)
                {
                    sum += predictIncrease;
                    dataContext.Add(new KeyValuePair<double, Size>(i - delta, new Size()));
                    dataContext.Add(new KeyValuePair<double, Size>(i, sum));
                    dataContext.Add(new KeyValuePair<double, Size>(i + delta, new Size()));
                }
                return dataContext;
            };
            if(data.Key.Month == DateTime.Now.Month && data.Key.Year == DateTime.Now.Year)
                DataContext = loadDataCurrent(data);
            else
                DataContext = loadDataPrevious(data);
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}

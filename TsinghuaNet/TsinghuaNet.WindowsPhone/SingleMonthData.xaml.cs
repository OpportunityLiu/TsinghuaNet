using TsinghuaNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Globalization;
using TsinghuaNet.Web;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SingleMonthData : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public SingleMonthData()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            for(int i = 0; i < 12; i++)
            {
                var item = (PivotItem)pivot.Items[i];
                item.Header = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i];
                item.Tag = i + 1;
            }
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get
            {
                return this.navigationHelper;
            }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get
            {
                return this.defaultViewModel;
            }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。 首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            KeyValuePair<DateTime, MonthlyData> data;
            try
            {
                data = (KeyValuePair<DateTime, MonthlyData>)e.NavigationParameter;
            }
            catch(InvalidCastException ex)
            {
                throw new InvalidOperationException("无法从此参数导航。", ex);
            }
            selectedYearMonth = data.Key;
            pivot.Title = selectedYearMonth.ToString("yyyy", CultureInfo.CurrentCulture);
            if(usageData != WebConnect.Current.UsageData.Traffic)
            {
                usageData = WebConnect.Current.UsageData.Traffic;
                monthlyDataCache.Clear();
            }
            if(pivot.SelectedIndex != selectedYearMonth.Month - 1)
                pivot.SelectedIndex = selectedYearMonth.Month - 1;
            else
                pivot_PivotItemLoading(pivot, new PivotItemEventArgs()
                {
                    Item = (PivotItem)pivot.SelectedItem
                });
        }

        private FrameworkElement getMonthDataChart(DateTime yearMonth, MonthlyData data)
        {
            if(monthlyDataChartCache.ContainsKey(yearMonth))
                return monthlyDataChartCache[yearMonth];
            var re = (FrameworkElement)((DataTemplate)Resources["ChartTemplate"]).LoadContent();
            re.DataContext = getMonthData(yearMonth, data);
            monthlyDataChartCache[yearMonth] = re;
            return re;
        }

        private Dictionary<DateTime, FrameworkElement> monthlyDataChartCache = new Dictionary<DateTime, FrameworkElement>();

        private List<KeyValuePair<double, Size>> getMonthData(DateTime yearMonth, MonthlyData data)
        {
            if(monthlyDataCache.ContainsKey(yearMonth))
                return monthlyDataCache[yearMonth];
            var delta = 1E-9;
            Func<DateTime, MonthlyData, List<KeyValuePair<double, Size>>> loadDataPrevious = (t, d) =>
            {
                var dataContext = new List<KeyValuePair<double, Size>>();
                var sum = Size.MinValue;
                var day = t;
                var dayMax = DateTime.DaysInMonth(day.Year, day.Month);
                for(double i = 1; i <= dayMax; i++)
                {
                    try
                    {
                        sum += d[day];
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
            }, loadDataCurrent = (t, d) =>
            {
                var dataContext = new List<KeyValuePair<double, Size>>();
                var sum = Size.MinValue;
                var day = t;
                var dayMax = DateTime.DaysInMonth(day.Year, day.Month);
                var dayNow = DateTime.Now.Day;
                var predictIncrease = d.Sum / dayNow;
                double i;
                for(i = 1d; i <= dayNow; i++)
                {
                    try
                    {
                        sum += d[day];
                    }
                    catch(KeyNotFoundException)
                    {
                    }
                    dataContext.Add(new KeyValuePair<double, Size>(i - delta, sum));
                    dataContext.Add(new KeyValuePair<double, Size>(i, new Size()));
                    dataContext.Add(new KeyValuePair<double, Size>(i + delta, sum));
                    day = day.AddDays(1);
                }
                dataContext.Add(new KeyValuePair<double, Size>(i - 1 + 2 * delta, new Size()));
                for(; i <= dayMax; i++)
                {
                    sum += predictIncrease;
                    dataContext.Add(new KeyValuePair<double, Size>(i - delta, new Size()));
                    dataContext.Add(new KeyValuePair<double, Size>(i, sum));
                    dataContext.Add(new KeyValuePair<double, Size>(i + delta, new Size()));
                }
                return dataContext;
            };
            var re = (yearMonth.Month == DateTime.Now.Month && yearMonth.Year == DateTime.Now.Year) ? loadDataCurrent(yearMonth, data) : loadDataPrevious(yearMonth, data);
            monthlyDataCache.Add(yearMonth, re);
            return re;
        }

        private Dictionary<DateTime, List<KeyValuePair<double, Size>>> monthlyDataCache = new Dictionary<DateTime, List<KeyValuePair<double, Size>>>();

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private DateTime selectedYearMonth;
        private IReadOnlyDictionary<DateTime, MonthlyData> usageData;

        private void pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            var item = args.Item;
            if(usageData.ContainsKey(selectedYearMonth))
                item.Content = getMonthDataChart(selectedYearMonth, usageData[selectedYearMonth]);
            else
                item.Content = "暂无数据";
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.RemovedItems.Count==0)
                return;
            var removedMonth = (int)((PivotItem)e.RemovedItems[0]).Tag;
            var addedMonth = (int)((PivotItem)e.AddedItems[0]).Tag;
            if(removedMonth == 1 && addedMonth == 12)
            {
                selectedYearMonth = selectedYearMonth.AddMonths(-1);
                pivot.Title = selectedYearMonth.ToString("yyyy", CultureInfo.CurrentCulture);
            }
            else if(removedMonth - 1 > addedMonth)
            {
                selectedYearMonth = new DateTime(selectedYearMonth.Year + 1, addedMonth, 1);
                pivot.Title = selectedYearMonth.ToString("yyyy", CultureInfo.CurrentCulture);
            }
            else
                selectedYearMonth = new DateTime(selectedYearMonth.Year, (int)((PivotItem)e.AddedItems[0]).Tag, 1);
        }
    }
}

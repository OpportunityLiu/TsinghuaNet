using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TsinghuaNet
{
    /// <summary>
    /// 表示历史流量的类。
    /// </summary>
    public class WebUsageData
    {
        /// <summary>
        /// 通过流量数据及当前设备信息统计流量以建立 <see cref="TsinghuaNet.WebUsageData"/> 的新实例。
        /// </summary>
        /// <param name="detailHtml">包含流量数据的 html 页。</param>
        /// <param name="devices">当前设备列表。</param>
        /// <exception cref="System.ArgumentNullException">参数为 <c>null</c>。</exception>
        public WebUsageData(string detailHtml, IEnumerable<WebDevice> devices)
        {
            if(string.IsNullOrEmpty(detailHtml))
                throw new ArgumentNullException("detailHtml");
            if(devices == null)
                throw new ArgumentNullException("devices");
            var trafficD = new Dictionary<DateTime, Size>();
            traffic = new Dictionary<DateTime, MonthlyData>();
            foreach(Match item in Regex.Matches(detailHtml, "\\<tr align=\"center\" style=.+?/tr\\>", RegexOptions.Singleline))
            {
                var lines = Regex.Matches(item.Value, "(?<=\\<td.+?\\>)(.+?)(?=\\</td\\>)");
                var date = DateTime.ParseExact(lines[3].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Date;
                if(trafficD.ContainsKey(date))
                    trafficD[date] += Size.Parse(lines[9].Value);
                else
                    trafficD[date] = Size.Parse(lines[9].Value);
            }
            foreach(var item in devices)
            {
                var date = item.LogOnDateTime.Date;
                if(trafficD.ContainsKey(date))
                    trafficD[date] += item.WebTraffic;
                else
                    trafficD[date] = item.WebTraffic;
            }
            var monthList = from item in trafficD
                            let date = item.Key
                            group item by new DateTime(date.Year, date.Month, 1);
            foreach(var item in monthList)
            {
                var monthlyData = new Dictionary<DateTime, Size>();
                foreach(var dailyData in item)
                    monthlyData.Add(dailyData.Key, dailyData.Value);
                traffic.Add(item.Key, new MonthlyData(monthlyData));
            }
            Traffic = new ReadOnlyDictionary<DateTime, MonthlyData>(traffic);
        }

        private Dictionary<DateTime, MonthlyData> traffic;

        /// <summary>
        /// 表示以月为单位统计的流量数据。
        /// </summary>
        public ReadOnlyDictionary<DateTime, MonthlyData> Traffic
        {
            get;
            private set;
        }

        public double Width
        {
            get
            {
                return Traffic.Count * 70;
            }
        }

        private class webDetailQuery
        {
            public webDetailQuery(WebDevice onlineDevice)
            {
                LogOnTime = onlineDevice.LogOnDateTime;
                //LogOffTime = DateTime.Now;
                WebTraffic = onlineDevice.WebTraffic;
                //Mac = onlineDevice.MacAddress;
            }

            public webDetailQuery(string partOfHtmlTable)
            {
                var list = Regex.Matches(partOfHtmlTable, "(?<=\\<td.+?\\>)(.+?)(?=\\</td\\>)");
                LogOnTime = DateTime.ParseExact(list[2].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                //LogOffTime = DateTime.ParseExact(list[3].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                WebTraffic = Size.Parse(list[9].Value);
                //Mac = MacAddress.Parse(list[13].Value);
            }

            public readonly DateTime LogOnTime;

            //public readonly DateTime LogOffTime;

            public readonly Size WebTraffic;

            //public readonly MacAddress Mac;
        }
    }

    public class MonthlyData : ReadOnlyDictionary<DateTime, Size>
    {
        public MonthlyData(Dictionary<DateTime, Size> dictionary)
            : base(dictionary)
        {
            Sum = dictionary.Values.Aggregate((a, b) => a + b);
        }

        public Size Sum
        {
            get;
            private set;
        }
    }
}

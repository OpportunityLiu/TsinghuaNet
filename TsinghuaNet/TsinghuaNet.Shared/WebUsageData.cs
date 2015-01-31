using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;

namespace TsinghuaNet
{
    public class WebUsageData
    {
        public WebUsageData(string detailHtml, IList<WebDevice> devices)
        {
            var t = DateTime.Now.Ticks;
            if(string.IsNullOrEmpty(detailHtml))
                throw new ArgumentNullException("detailHtml");
            traffic = new Dictionary<DateTime, Size>();
            trafficM = new Dictionary<int, Size>();
            foreach(Match item in Regex.Matches(detailHtml, "\\<tr align=\"center\" style=.+?/tr\\>", RegexOptions.Singleline))
            {
                var lines = Regex.Matches(item.Value, "(?<=\\<td.+?\\>)(.+?)(?=\\</td\\>)");
                var date = DateTime.ParseExact(lines[3].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Date;
                if(traffic.ContainsKey(date))
                    traffic[date] += Size.Parse(lines[9].Value);
                else
                    traffic[date] = Size.Parse(lines[9].Value);
            }
            foreach(var item in devices)
            {
                var date = item.LogOnDateTime.Date;
                if(traffic.ContainsKey(date))
                    traffic[date] += item.WebTraffic;
                else
                    traffic[date] = item.WebTraffic;
            }
            var monthList = from item in traffic
                            group item.Value by item.Key.Month + item.Key.Year * 100;
            foreach(var item in monthList)
                trafficM[item.Key] = item.Aggregate((size1, size2) => size1 + size2);
            System.Diagnostics.Debug.WriteLine(DateTime.Now.Ticks - t);
        }

        public Dictionary<DateTime, Size> traffic;
        public Dictionary<int, Size> trafficM;

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


}

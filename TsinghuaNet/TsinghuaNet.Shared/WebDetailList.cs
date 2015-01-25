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
    public class WebDetailList
    {
        public WebDetailList(string detailHtml, IList<WebDevice> devices)
        {
            if(string.IsNullOrEmpty(detailHtml))
                throw new ArgumentNullException("detailHtml");
            traffic = new Dictionary<DateTime, Size>();
            trafficM = new Dictionary<int, Size>();
            var detailList = new List<webDetailQuery>();
            foreach(var item in devices)
                detailList.Add(new webDetailQuery(item));
            foreach(Match item in Regex.Matches(detailHtml, "\\<tr align=\"center\" style=.+?/tr\\>", RegexOptions.Singleline))
                detailList.Add(new webDetailQuery(item.Value));
            var queryOfDay = from item in detailList
                    group item.WebTraffic by item.LogOnTime.Date;
            foreach(var item in queryOfDay)
            {
                var sum = new Size();
                foreach(var item2 in item)
                    sum += item2;
                traffic[item.Key] = sum;
            }
            var queryOfMonth = from item in traffic
                               group item.Value by item.Key.Year * 100 + item.Key.Month;
            foreach(var item in queryOfMonth)
            {
                var sum = new Size();
                foreach(var item2 in item)
                    sum += item2;
                trafficM[item.Key] = sum;
            }
        }

        private Dictionary<DateTime, Size> traffic;
        private Dictionary<int, Size> trafficM;

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

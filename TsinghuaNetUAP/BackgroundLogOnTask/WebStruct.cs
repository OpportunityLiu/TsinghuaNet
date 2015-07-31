using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Storage.Streams;

namespace BackgroundLogOnTask
{
    /// <summary>
    /// 表示一定的字节数。
    /// </summary>
    internal sealed class Size
    {

        private const double kb = 1e3;
        private const double mb = 1e6;
        private const double gb = 1e9;
        private const double tb = 1e12;
        private const double pb = 1e15;

        /// <summary>
        /// 将字节数的字符串表示形式转换为它的等效 <see cref="Size"/>。
        /// </summary>
        /// <param name="sizeString">包含要转换的数字的字符串。</param>
        /// <returns>与 <paramref name="sizeString"/> 中指定的数值或符号等效的 <see cref="Size"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sizeString"/> 为 <c>null</c>。</exception>
        /// <exception cref="FormatException"><paramref name="sizeString"/> 不表示一个有效格式的数字。</exception>
        public static Size Parse(string sizeString)
        {
            if(sizeString == null)
                throw new ArgumentNullException("sizeString");
            if(string.IsNullOrWhiteSpace(sizeString) || sizeString.Length == 1)
                throw new FormatException("字符串格式错误。");
            switch(sizeString[sizeString.Length - 1])
            {
                case 'P':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture) * pb));
                case 'T':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture) * tb));
                case 'G':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture) * gb));
                case 'M':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture) * mb));
                case 'K':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture) * kb));
                case 'B':
                    return new Size((ulong)(double.Parse(sizeString.Substring(0, sizeString.Length - 1), CultureInfo.InvariantCulture)));
                default:
                    throw new FormatException("字符串格式错误。");
            }
        }

        /// <summary>
        /// 指示此实例与指定对象是否相等。
        /// </summary>
        /// <param name="obj">要与当前实例进行比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public sealed override bool Equals(object obj)
        {
            if(obj is Size)
                return this == (Size)obj;
            else
                return false;
        }

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public sealed override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// 创建 <see cref="Size"/> 的新实例。
        /// </summary>
        /// <param name="sizeString">要存储的字节数。</param>
        public Size(ulong sizeValue)
        {
            this.Value = sizeValue;
        }

        /// <summary>
        /// 存储的值。
        /// </summary>
        public ulong Value
        {
            get;
            set;
        }

        public double TotalGB
        {
            get
            {
                return Value / gb;
            }
        }

        /// <summary>
        /// 返回当前对象的字符串形式。
        /// </summary>
        /// <returns>当前对象的字符串形式。</returns>
        public sealed override string ToString()
        {
            var re = "";
            var va = Value;
            if(va < kb)
                re = va + " B";
            else if(va < mb)
                re = (va / kb).ToString("0.00", CultureInfo.InvariantCulture) + " KB";
            else if(va < gb)
                re = (va / mb).ToString("0.00", CultureInfo.InvariantCulture) + " MB";
            else if(va < tb)
                re = (va / gb).ToString("0.00", CultureInfo.InvariantCulture) + " GB";
            else if(va < pb)
                re = (va / tb).ToString("0.00", CultureInfo.InvariantCulture) + " TB";
            else
                re = (va / pb).ToString("0.00", CultureInfo.InvariantCulture) + " PB";
            return re;
        }
    }


    /// <summary>
    /// 表示特定的 Mac 地址。
    /// </summary>
    internal struct MacAddress
    {
        /// <summary>
        /// 表示本机的 <see cref="MacAddress"/> 对象。
        /// 此字段为只读。
        /// </summary>
        public static readonly MacAddress Current = initCurrentMac();

        private static MacAddress initCurrentMac()
        {
            var id = Windows.Networking.Connectivity.NetworkInformation.FindConnectionProfilesAsync(new Windows.Networking.Connectivity.ConnectionProfileFilter() { IsConnected = true }).AsTask().Result;
            var b = id.First().NetworkAdapter.NetworkAdapterId.ToByteArray();
            var r = new byte[6];
            Array.Copy(b, 10, r, 0, 6);
            return new MacAddress(r);
        }

        /// <summary>
        /// 表示未知的 <see cref="MacAddress"/>。
        /// 此字段为只读。
        /// </summary>
        public static readonly MacAddress Unknown = new MacAddress();

        /// <summary>
        /// 通过字节数组创建 <see cref="MacAddress"/> 的新实例。
        /// </summary>
        /// <param name="sizeString">长度为 6 的 <see cref="System.Byte[]"/>，表示一个 Mac 地址。</param>
        /// <exception cref="ArgumentNullException"><paramref name="sizeString"/> 为 <c>null</c>。</exception>
        /// <exception cref="System.ArgumentException"><paramref name="sizeString"/> 长度不为 6。</exception>
        public MacAddress(params byte[] value)
        {
            if(value == null)
                throw new ArgumentNullException("value");
            if(value.Length != 6)
                throw new ArgumentException("数组长度应为 6。", "value");
            this.value0 = value[0];
            this.value1 = value[1];
            this.value2 = value[2];
            this.value3 = value[3];
            this.value4 = value[4];
            this.value5 = value[5];
        }

        /// <summary>
        /// 获取或设置 <see cref="MacAddress"/> 的值。
        /// </summary>
        /// <param name="index">索引，0~5。</param>
        /// <returns><see cref="MacAddress"/> 相应位的值。</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> 超出索引范围。</exception>
        public byte this[int index]
        {
            get
            {
                switch(index)
                {
                case 0:
                    return value0;
                case 1:
                    return value1;
                case 2:
                    return value2;
                case 3:
                    return value3;
                case 4:
                    return value4;
                case 5:
                    return value5;
                default:
                    throw new ArgumentOutOfRangeException("index", "index 应为0~5。");
                }
            }
            private set
            {
                switch(index)
                {
                case 0:
                    value0 = value;
                    break;
                case 1:
                    value1 = value;
                    break;
                case 2:
                    value2 = value;
                    break;
                case 3:
                    value3 = value;
                    break;
                case 4:
                    value4 = value;
                    break;
                case 5:
                    value5 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("index", "index 应为0~5。");
                }
            }
        }

        private byte value0, value1, value2, value3, value4, value5;

        /// <summary>
        /// 返回当前 <see cref="MacAddress"/> 的字符串形式。
        /// </summary>
        /// <returns>当前 <see cref="MacAddress"/> 的字符串形式，以 ":" 分隔。</returns>
        public override string ToString()
        {
            return $"{value0:X2}:{value1:X2}:{value2:X2}:{value3:X2}:{value4:X2}:{value5:X2}";
        }

        /// <summary>
        /// 指示此实例与指定对象是否相等。
        /// </summary>
        /// <param name="obj">要与当前实例进行比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool Equals(object obj)
        {
            if(obj is MacAddress)
            {
                var value = (MacAddress)obj;
                var flag = true;
                for(int i = 0; i < 6; i++)
                {
                    if(this[i] != value[i])
                    {
                        flag = false;
                        break;
                    }
                }
                return flag;
            }
            else
                return false;
        }

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            int re = (((value2 << 8) + value3 << 8) + value4 << 8) + value5;
            int re2 = value0 + (value1 << 8);
            return re ^ re2;
        }

        public static bool operator ==(MacAddress mac1, MacAddress mac2)
        {
            return mac1.Equals(mac2);
        }

        public static bool operator !=(MacAddress mac1, MacAddress mac2)
        {
            return !mac1.Equals(mac2);
        }
    }
}
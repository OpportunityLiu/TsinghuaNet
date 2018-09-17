using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Web
{
    /// <summary>
    /// 表示一定的字节数。
    /// </summary>
    public readonly struct Size : IComparable<Size>, IEquatable<Size>
    {
        /// <summary>
        /// 表示 <see cref="Size"/> 的最小可能值。
        /// 此字段为只读。
        /// </summary>
        public static readonly Size MinValue = new Size(ulong.MinValue);

        /// <summary>
        /// 表示 <see cref="Size"/> 的最大可能值。
        /// 此字段为只读。
        /// </summary>
        public static readonly Size MaxValue = new Size(ulong.MaxValue);

        private const double kb = 1e3;
        private const double mb = 1e6;
        private const double gb = 1e9;
        private const double tb = 1e12;
        private const double pb = 1e15;

        /// <summary>
        /// 将字节数的字符串表示形式转换为它的等效 <see cref="Size"/>。
        /// </summary>
        /// <param name="value">包含要转换的数字的字符串。</param>
        /// <returns>与 <paramref name="value"/> 中指定的数值或符号等效的 <see cref="Size"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <c>null</c>。</exception>
        /// <exception cref="FormatException"><paramref name="value"/> 不表示一个有效格式的数字。</exception>
        public static Size Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException("字符串格式错误。");
            switch (value[value.Length - 1])
            {
            case 'P':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) * pb));
            case 'T':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) * tb));
            case 'G':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) * gb));
            case 'M':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) * mb));
            case 'K':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture) * kb));
            case 'B':
                return new Size((ulong)(double.Parse(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture)));
            default:
                return new Size((ulong)(double.Parse(value, CultureInfo.InvariantCulture)));
            }
        }

        public static bool operator <=(Size size1, Size size2) => size1.Value <= size2.Value;

        public static bool operator >=(Size size1, Size size2) => size1.Value >= size2.Value;

        public static bool operator <(Size size1, Size size2) => size1.Value < size2.Value;

        public static bool operator >(Size size1, Size size2) => size1.Value > size2.Value;

        public static Size operator +(Size size1, Size size2) => new Size(size1.Value + size2.Value);

        public static Size operator *(Size size1, double d2) => new Size((ulong)(size1.Value * d2));

        public static Size operator /(Size size1, double d2) => new Size((ulong)(size1.Value / d2));

        public static Size operator -(Size size1, Size size2)
        {
            if (size1 < size2)
                throw new OverflowException("size1 < size2");
            return new Size(size1.Value - size2.Value);
        }

        public static bool operator ==(Size size1, Size size2) => size1.Value == size2.Value;

        public static bool operator !=(Size size1, Size size2) => size1.Value != size2.Value;

        /// <summary>
        /// 指示此实例与指定对象是否相等。
        /// </summary>
        /// <param name="obj">要与当前实例进行比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool Equals(object obj) => obj is Size s && Equals(s);

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode() => this.Value.GetHashCode();

        /// <summary>
        /// 创建 <see cref="Size"/> 的新实例。
        /// </summary>
        /// <param name="sizeString">要存储的字节数。</param>
        public Size(ulong value) => this.Value = value;

        /// <summary>
        /// 存储的值。
        /// </summary>
        public ulong Value { get; }

        public double TotalGB => this.Value / gb;

        /// <summary>
        /// 返回当前对象的字符串形式。
        /// </summary>
        /// <returns>当前对象的字符串形式。</returns>
        public override string ToString()
        {
            var va = this.Value;
            if (va < kb)
                return va.ToString(CultureInfo.CurrentCulture) + " B";
            if (va < mb)
                return format(va / kb) + " KB";
            if (va < gb)
                return format(va / mb) + " MB";
            if (va < tb)
                return format(va / gb) + " GB";
            if (va < pb)
                return format(va / tb) + " TB";
            return format(va / pb) + " PB";

            string format(double value)
            {
                return value.ToString("##0.00", CultureInfo.CurrentCulture);
            }
        }

        public int CompareTo(Size other) => this.Value.CompareTo(other.Value);

        public bool Equals(Size other) => this == other;
    }

    /// <summary>
    /// 表示特定的 Mac 地址。
    /// </summary>
    public unsafe struct MacAddress : IEquatable<MacAddress>
    {
        private static readonly char[] splitChars = ":. ".ToCharArray();

        /// <summary>
        /// 将 Mac 地址的字符串表示形式转换为它的等效 <see cref="MacAddress"/>。
        /// </summary>
        /// <param name="value">包含要转换的 Mac 地址的字符串。</param>
        /// <returns>与 <paramref name="value"/> 中指定的 <see cref="MacAddress"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="FormatException"><paramref name="value"/> 不表示一个有效格式的 Mac 地址。</exception>
        public static MacAddress Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Unknown;
            var mac = value.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (mac.Length != 6)
                throw new FormatException("字符串格式有误。");
            var result = new MacAddress();
            try
            {
                for (var i = 0; i < 6; i++)
                    result.value[i] = byte.Parse(mac[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new FormatException("字符串格式有误。", ex);
            }
            return result;
        }

        /// <summary>
        /// 表示本机的 <see cref="MacAddress"/> 对象。
        /// 此字段为只读。
        /// </summary>
        public static readonly MacAddress Current = initCurrentMac();

        private static MacAddress initCurrentMac()
        {
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.TryGetValue("Mac", out var mac))
            {
                return Parse(mac.ToString());
            }
            else
            {
                var bytes = new byte[6];
                new Random().NextBytes(bytes);
                var re = new MacAddress(bytes);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values.Add("Mac", re.ToString());
                return re;
            }
        }

        /// <summary>
        /// 表示未知的 <see cref="MacAddress"/>。
        /// 此字段为只读。
        /// </summary>
        public static readonly MacAddress Unknown = new MacAddress();

        /// <summary>
        /// 通过字节数组创建 <see cref="MacAddress"/> 的新实例。
        /// </summary>
        /// <param name="sizeString">长度为 6 的 <see cref="byte[]"/>，表示一个 Mac 地址。</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> 长度不为 6。</exception>
        public MacAddress(params byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length != 6)
                throw new ArgumentException("数组长度应为 6。", "value");

            for (var i = 0; i < 6; i++)
                this.value[i] = value[i];
        }

        /// <summary>
        /// 获取或设置 <see cref="MacAddress"/> 的值。
        /// </summary>
        /// <param name="index">索引，0~5。</param>
        /// <returns><see cref="MacAddress"/> 相应位的值。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 超出索引范围。</exception>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= 6)
                    throw new ArgumentOutOfRangeException("index", "index 应为0~5。");
                return this.value[index];
            }
        }

        private fixed byte value[6];

        /// <summary>
        /// 获取一个值，指示当前 <see cref="MacAddress"/> 是否与 <see cref="Current"/> 相等。
        /// </summary>
        public bool IsCurrent => this == Current;

        /// <summary>
        /// 返回当前 <see cref="MacAddress"/> 的字符串形式。
        /// </summary>
        /// <returns>当前 <see cref="MacAddress"/> 的字符串形式，以 ":" 分隔。</returns>
        public override string ToString()
        {
            return $"{this.value[0]:x2}:{this.value[1]:x2}:{this.value[2]:x2}:{this.value[3]:x2}:{this.value[4]:x2}:{this.value[5]:x2}";
        }

        /// <summary>
        /// 指示此实例与指定对象是否相等。
        /// </summary>
        /// <param name="obj">要与当前实例进行比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool Equals(object obj) => obj is MacAddress mac && Equals(mac);

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            var re = (((this.value[2] << 8) + this.value[3] << 8) + this.value[4] << 8) + this.value[5];
            var re2 = this.value[0] + (this.value[1] << 8);
            return re ^ (re2 * 91019);
        }

        public bool Equals(MacAddress other)
        {
            for (var i = 0; i < 6; i++)
            {
                if (this.value[i] != other.value[i])
                    return false;
            }
            return true;
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

    /// <summary>
    /// 表示特定的 Ipv4 地址。
    /// </summary>
    public unsafe struct Ipv4Address : IEquatable<Ipv4Address>
    {
        private static readonly char[] splitChars = ".: ".ToCharArray();

        /// <summary>
        /// 将 IP 地址的字符串表示形式转换为它的等效 <see cref="Ipv4Address"/>。
        /// </summary>
        /// <param name="sizeString">包含要转换的 IP 地址的字符串。</param>
        /// <returns>与 <paramref name="sizeString"/> 中指定的 <see cref="Ipv4Address"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="FormatException"><paramref name="value"/> 不表示一个有效格式的 IP 地址。</exception>
        public static Ipv4Address Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("value");
            var ip = value.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (ip.Length != 4)
                throw new FormatException("字符串格式有误。");
            var result = new Ipv4Address();
            try
            {
                for (var i = 0; i < 4; i++)
                    result.value[i] = byte.Parse(ip[i], CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new FormatException("字符串格式有误。", ex);
            }
            return result;
        }

        /// <summary>
        /// 通过字节数组创建 <see cref="Ipv4Address"/> 的新实例。
        /// </summary>
        /// <param name="value">长度为 4 的 <see cref="byte[]"/>，表示一个 IP 地址。</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> 长度不为 4。</exception>
        public Ipv4Address(params byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length != 4)
                throw new ArgumentException("数组长度应为 4。", "value");

            for (var i = 0; i < 4; i++)
                this.value[i] = value[i];
        }

        /// <summary>
        /// 获取或设置 <see cref="Ipv4Address"/> 的值。
        /// </summary>
        /// <param name="index">索引，0~3。</param>
        /// <returns><see cref="Ipv4Address"/> 相应位的值。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 超出索引范围。</exception>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= 4)
                    throw new ArgumentOutOfRangeException("index", "index 应为0~3。");
                return this.value[index];
            }
        }

        private fixed byte value[4];

        /// <summary>
        /// 返回当前 <see cref="Ipv4Address"/> 的字符串形式。
        /// </summary>
        /// <returns>当前 <see cref="Ipv4Address"/> 的字符串形式，以 "." 分隔。</returns>
        public override string ToString()
            => $"{this.value[0]}.{this.value[1]}.{this.value[2]}.{this.value[3]}";

        /// <summary>
        /// 指示此实例与指定对象是否相等。
        /// </summary>
        /// <param name="obj">要与当前实例进行比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 和该实例具有相同的类型并表示相同的值，则为 <c>true</c>；否则为 <c>false</c>。
        /// </returns>
        public override bool Equals(object obj) => obj is Ipv4Address add && Equals(add);

        public bool Equals(Ipv4Address other)
        {
            for (var i = 0; i < 4; i++)
            {
                if (this.value[i] != other.value[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>一个 32 位有符号整数，它是该实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            var re = 0;
            for (var i = 0; i < 4; i++)
            {
                re <<= 8;
                re += this.value[i];
            }
            return re;
        }

        public static bool operator ==(Ipv4Address ip1, Ipv4Address ip2) => ip1.Equals(ip2);

        public static bool operator !=(Ipv4Address ip1, Ipv4Address ip2) => !ip1.Equals(ip2);
    }
}
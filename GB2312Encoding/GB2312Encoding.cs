using System;
using System.IO;
using System.Text;
using Windows.Storage;
using Windows.Storage.Streams;

namespace GB2312
{
    internal sealed class GB2312Encoding : System.Text.Encoding
    {
        private const char LEAD_BYTE_CHAR = '\uFFFE';
        private char[] gbToUnicode = new char[0x10000];
        private ushort[] unicodeToGb = new ushort[0x10000];
        private string _webName = "gb2312";

        public GB2312Encoding()
        {
            if(!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported big endian platform.");
            var file = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///GB2312Encoding/gb2312.bin")).AsTask().Result;
            using(var reader = new BinaryReader(file.OpenSequentialReadAsync().AsTask().Result.AsStreamForRead()))
            {
                for(int i = 0; i < 0xffff; i++)
                {
                    ushort u = reader.ReadUInt16();
                    this.unicodeToGb[i] = u;
                }
                for(int i = 0; i < 0xffff; i++)
                {
                    ushort u = reader.ReadUInt16();
                    this.gbToUnicode[i] = (char)u;
                }
            }
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            if(chars == null)
                throw new ArgumentNullException("chars");
            int byteCount = 0;
            ushort u;
            char c;

            for(int i = 0; i < count; index++, byteCount++, i++)
            {
                c = chars[index];
                u = unicodeToGb[c];
                if(u > 0xff)
                    byteCount++;
            }

            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if(chars == null)
                throw new ArgumentNullException("chars");
            if(bytes == null)
                throw new ArgumentNullException("bytes");
            int byteCount = 0;
            ushort u;
            char c;

            for(int i = 0; i < charCount; charIndex++, byteIndex++, byteCount++, i++)
            {
                c = chars[charIndex];
                u = unicodeToGb[c];
                if(u == 0 && c != 0)
                {
                    bytes[byteIndex] = 0x3f;    // 0x3f == '?'
                }
                else if(u < 0x100)
                {
                    bytes[byteIndex] = (byte)u;
                }
                else
                {
                    bytes[byteIndex] = (byte)((u >> 8) & 0xff);
                    byteIndex++;
                    byteCount++;
                    bytes[byteIndex] = (byte)(u & 0xff);
                }
            }

            return byteCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetCharCount(bytes, index, count, null);
        }

        private int GetCharCount(byte[] bytes, int index, int count, GB2312Decoder decoder)
        {
            int charCount = 0;
            ushort u;
            char c;

            for(int i = 0; i < count; index++, charCount++, i++)
            {
                u = 0;
                if(decoder != null && decoder.pendingByte != 0)
                {
                    u = decoder.pendingByte;
                    decoder.pendingByte = 0;
                }

                u = (ushort)(u << 8 | bytes[index]);
                c = gbToUnicode[u];
                if(c == LEAD_BYTE_CHAR)
                {
                    if(i < count - 1)
                    {
                        index++;
                        i++;
                    }
                    else if(decoder != null)
                    {
                        decoder.pendingByte = bytes[index];
                        return charCount;
                    }
                }
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, null);
        }

        private int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, GB2312Decoder decoder)
        {
            int charCount = 0;
            ushort u;
            char c;

            for(int i = 0; i < byteCount; byteIndex++, charIndex++, charCount++, i++)
            {
                u = 0;
                if(decoder != null && decoder.pendingByte != 0)
                {
                    u = decoder.pendingByte;
                    decoder.pendingByte = 0;
                }

                u = (ushort)(u << 8 | bytes[byteIndex]);
                c = gbToUnicode[u];
                if(c == LEAD_BYTE_CHAR)
                {
                    if(i < byteCount - 1)
                    {
                        byteIndex++;
                        i++;
                        u = (ushort)(u << 8 | bytes[byteIndex]);
                        c = gbToUnicode[u];
                    }
                    else if(decoder == null)
                    {
                        c = '\0';
                    }
                    else
                    {
                        decoder.pendingByte = bytes[byteIndex];
                        return charCount;
                    }
                }
                if(c == 0 && u != 0)
                    chars[charIndex] = '?';
                else
                    chars[charIndex] = c;
            }

            return charCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if(charCount < 0)
                throw new ArgumentOutOfRangeException("charCount");
            long count = charCount + 1;
            count *= 2;
            if(count > int.MaxValue)
                throw new ArgumentOutOfRangeException("charCount");
            return (int)count;

        }

        public override int GetMaxCharCount(int byteCount)
        {
            if(byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount");
            long count = byteCount + 3;
            if(count > int.MaxValue)
                throw new ArgumentOutOfRangeException("byteCount");
            return (int)count;
        }

        public override Decoder GetDecoder()
        {
            return new GB2312Decoder(this);
        }

        public override string WebName
        {
            get
            {
                return _webName;
            }
        }

        private sealed class GB2312Decoder : Decoder
        {
            private GB2312Encoding encoding = null;

            public GB2312Decoder(GB2312Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                return encoding.GetCharCount(bytes, index, count, this);
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                return encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex, this);
            }

            public byte pendingByte;
        }
    }

    /// <summary>
    /// 表示字符编码。
    /// </summary>
    public abstract class Encoding : System.Text.Encoding
    {
        private static System.Text.Encoding encoding = new GB2312Encoding();

        /// <summary>
        /// 获取 GB2312 格式的编码。
        /// </summary>
        public static System.Text.Encoding GB2312
        {
            get
            {
                return encoding;
            }
        }
    }
}

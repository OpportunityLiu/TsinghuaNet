using System;
using System.Collections.Generic;
using System.Text;
using Windows.Data.Json;

namespace TsinghuaNet.Common
{
    public class SerializableList<T>:List<T>
    {
        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        public SerializableList(ItemSerializer<T> serializer,ItemDeserializer<T> deserializer)
            : base()
        {
            if(serializer == null)
                throw new ArgumentNullException("serializer");
            if(deserializer == null)
                throw new ArgumentNullException("deserializer");
            this.serializer = serializer;
            this.deserializer = deserializer;
        }

        /// <summary>
        /// 使用序列化字符串初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        /// <param name="jsonInput">保存有字典信息的 json 序列化字符串。</param>
        /// <exception cref="System.ArgumentException">输入字符串有误，无法进行反序列化。</exception>
        public SerializableList(ItemSerializer<T> serializer,ItemDeserializer<T> deserializer,string jsonInput)
            : this(serializer,deserializer)
        {
            try
            {
                foreach(var item in JsonArray.Parse(jsonInput))
                {
                    Add(deserializer(item.GetString()));
                }
            }
            catch(Exception ex)
            {
                throw new ArgumentException("输入有误。", "jsonInput", ex);
            }
        }

        private ItemSerializer<T> serializer;
        private ItemDeserializer<T> deserializer;

        /// <summary>
        /// 对当前实例进行序列化。
        /// </summary>
        /// <returns>序列化后的字符串。</returns>
        public string Serialize()
        {
            var jsonData = new JsonArray();
            foreach(var item in this)
            {
                jsonData.Add(JsonValue.CreateStringValue(serializer(item)));
            }
            return jsonData.Stringify();
        }
    }

    public delegate string ItemSerializer<T>(T item);

    public delegate T ItemDeserializer<T>(string item);
}

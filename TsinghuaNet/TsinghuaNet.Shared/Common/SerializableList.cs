using System;
using System.Collections.Generic;
using System.Text;
using Windows.Data.Json;
using System.Reflection;
using System.Linq;

namespace TsinghuaNet.Common
{
    /// <summary>
    /// 表示可以进行序列化的强类型列表。提供用于对列表进行搜索、排序和操作的方法。
    /// </summary>
    /// <typeparam name="T">列表中元素的类型。</typeparam>
    public class SerializableList<T> : List<T>
    {
        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        public SerializableList()
        {
        }

        /// <summary>
        /// 使用序列化字符串初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        /// <param name="jsonInput">保存有字典信息的 json 序列化字符串。</param>
        /// <param name="deserializer">对列表中元素进行反序列化时使用的反序列化器。</param>
        /// <exception cref="System.ArgumentException">输入字符串有误，无法进行反序列化。</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="deserializer"/> 为 <c>null</c>。</exception>
        public static SerializableList<T> Deserialize(Deserializer<T> deserializer,string jsonInput)
        {
            if(deserializer == null)
                throw new ArgumentNullException("deserializer");
            try
            {
                var query = from item in JsonArray.Parse(jsonInput)
                            select deserializer(item.GetString());
                var re = new SerializableList<T>();
                re.AddRange(query);
                return re;
            }
            catch(Exception ex)
            {
                throw new ArgumentException("输入有误。", "jsonInput", ex);
            }
        }

        /// <summary>
        /// 对当前实例进行序列化。
        /// </summary>
        /// <param name="serializer">对列表中元素进行序列化时使用的序列化器。</param>
        /// <returns>序列化后的字符串。</returns>
        public string Serialize(Serializer<T> serializer)
        {
            var jsonData = new JsonArray();
            foreach(var item in this)
            {
                jsonData.Add(JsonValue.CreateStringValue(serializer(item)));
            }
            return jsonData.Stringify();
        }
    }

    /// <summary>
    /// 提供一种将指定类型序列化的方法。
    /// </summary>
    /// <typeparam name="T">要序列化的类型。</typeparam>
    /// <param name="item">要序列化的 <typeparamref name="T"/> 的实例。</param>
    /// <returns>一个字符串，表示序列化的结果。</returns>
    public delegate string Serializer<T>(T item);

    /// <summary>
    /// 提供一种将指定类型反序列化的方法。
    /// </summary>
    /// <typeparam name="T">要反序列化的类型。</typeparam>
    /// <param name="item">要反序列化的表示 <typeparamref name="T"/> 的字符串。</param>
    /// <returns>反序列化得到的 <typeparamref name="T"/> 的实例。</returns>
    public delegate T Deserializer<T>(string item);
}

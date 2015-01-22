using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Data.Json;

namespace TsinghuaNet
{
    /// <summary>
    /// 可序列化的设备 Mac 和名称的词典。
    /// </summary>
    public class DeviceNameDictionary : Dictionary<MacAddress, string>
    {
        /// <summary>
        /// 初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        public DeviceNameDictionary()
            : base()
        {
        }

        /// <summary>
        /// 使用序列化字符串初始化 <see cref="TsinghuaNet.DeviceNameDictionary"/> 的新实例。
        /// </summary>
        /// <param name="jsonInput">保存有字典信息的 json 序列化字符串。</param>
        /// <exception cref="System.ArgumentException">输入字符串有误，无法进行反序列化。</exception>
        public DeviceNameDictionary(string jsonInput)
            : base()
        {
            try
            {
                foreach(var item in JsonObject.Parse(jsonInput))
                {
                    this.Add(MacAddress.Parse(item.Key), item.Value.GetString());
                }
            }
            catch(Exception ex)
            {
                throw new ArgumentException("输入有误。", "jsonInput", ex);
            }
        }

        /// <summary>
        /// 对当前实例进行序列化。
        /// </summary>
        /// <returns>序列化后的字符串。</returns>
        public string Stringify()
        {
            var jsonData = new JsonObject();
            foreach(var item in this)
            {
                jsonData.Add(item.Key.ToString(), JsonValue.CreateStringValue(item.Value));
            }
            return jsonData.Stringify();
        }
    }
}

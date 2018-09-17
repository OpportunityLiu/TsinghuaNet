using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Settings
{
    public static class SettingsHelper
    {
        private static IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;
        private static IPropertySet roamingSettings = ApplicationData.Current.RoamingSettings.Values;

        public static T GetLocal<T>(string key)
        {
            return GetLocal(key, default(T));
        }

        public static T GetLocal<T>(string key, T defaultValue)
        {
            if (localSettings.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            else
            {
                localSettings[key] = defaultValue;
                return defaultValue;
            }
        }

        public static T GetRoaming<T>(string key)
        {
            return GetRoaming(key, default(T));
        }

        public static T GetRoaming<T>(string key, T defaultValue)
        {
            if (roamingSettings.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            else
            {
                roamingSettings[key] = defaultValue;
                return defaultValue;
            }
        }

        public static void SetLocal(string key, object value)
        {
            localSettings[key] = value;
        }

        public static void SetRoaming(string key, object value)
        {
            roamingSettings[key] = value;
        }
    }
}

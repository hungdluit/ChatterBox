using Windows.Foundation.Collections;

namespace ChatterBox.Client.Settings
{
    public static class SettingsExtensions
    {
        public static void AddOrUpdate(this IPropertySet propertySet, string key, object value)
        {
            if (propertySet.ContainsKey(key))
            {
                propertySet[key] = value;
            }
            else
            {
                propertySet.Add(key, value);
            }
        }
    }
}

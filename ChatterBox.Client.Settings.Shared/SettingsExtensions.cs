using Windows.Foundation.Collections;

namespace ChatterBox.Client.Settings.Shared
{
    public static class SettingsExtensions
    {
        public static void AddOrUpdate(this IPropertySet propertySet, string key, string value)
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

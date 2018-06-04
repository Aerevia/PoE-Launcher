using System;
using System.ComponentModel;
using System.Configuration;

namespace PoELauncher.Extensions
{
    [TypeConverter(typeof(SettingsConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ToolSettings
    {
        public bool Enabled { get; set; } = false;
        public bool Downloaded { get; set; } = false;
        public string CurrentVersion { get; set; } = String.Empty;
        public string LatestVersion { get; set; } = String.Empty;
    }

    public class SettingsConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] parts = ((string)value).Split(new char[] { ',' });
                ToolSettings settings = new ToolSettings();
                if (parts[0] != null)
                {
                    settings.Downloaded = Convert.ToBoolean(parts[0]);
                }
                if (parts[1] != null)
                {
                    settings.Enabled = Convert.ToBoolean(parts[1]);
                }
                if (parts[2] != null)
                {
                    settings.LatestVersion = parts[2];
                }
                return settings;
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ToolSettings settings = value as ToolSettings;
                return string.Format("{0},{1},{2}", settings.Downloaded.ToString(), settings.Enabled.ToString(), settings.LatestVersion);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}

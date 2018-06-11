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
}

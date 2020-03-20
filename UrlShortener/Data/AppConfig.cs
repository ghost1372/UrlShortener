using HandyControl.Data;
using System;

namespace UrlShortener
{
    internal class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public bool IsShowNotifyIcon { get; set; } = false;
        public bool IsFirstRun { get; set; } = true;
        public int SelectedIndex { get; set; } = 0;

        public SkinType Skin { get; set; } = SkinType.Default;
    }
}
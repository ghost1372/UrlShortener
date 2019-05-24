using HandyControl.Data;
using Newtonsoft.Json;
using System;
using System.IO;

namespace UrlShortener
{
    internal class GlobalData
    {
        internal class AppConfig
        {
            public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

            public bool TopMost { get; set; } = true;
            public bool FirstRun { get; set; } = true;
            public bool NotifyIconIsShow { get; set; } = true;
            public int ServiceIndex { get; set; } = 0;
            public SkinType Skin { get; set; } = SkinType.Violet;
        }
        public static AppConfig Config { get; set; }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(AppConfig.SavePath, json);
        }

        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    string json = File.ReadAllText(AppConfig.SavePath);
                    Config = JsonConvert.DeserializeObject<AppConfig>(json);
                }
                catch
                {
                    Config = new AppConfig();
                }
            }
            else
            {
                Config = new AppConfig();
            }
        }

    }
}

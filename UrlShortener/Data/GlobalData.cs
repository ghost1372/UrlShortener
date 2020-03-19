using Newtonsoft.Json;
using System;
using System.IO;

namespace UrlShortener
{
    internal class GlobalData
    {
        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    string json = File.ReadAllText(AppConfig.SavePath);
                    Config = (string.IsNullOrEmpty(json) ? new AppConfig() : JsonConvert.DeserializeObject<AppConfig>(json)) ?? new AppConfig();

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

        public static void Save()
        {
            try
            {

                string json = JsonConvert.SerializeObject(Config);
                File.WriteAllText(AppConfig.SavePath, json);
            }
            catch (UnauthorizedAccessException)
            {
                HandyControl.Controls.MessageBox.Error("you dont have administrator access, please run app as administrator", "Administrator Access Error");
            }
        }

        public static AppConfig Config { get; set; }

    }
}
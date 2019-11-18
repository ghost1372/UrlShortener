using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Windows;

namespace UrlShortener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (checkSkinType(InIHelper.ReadValue("Skin")) != SkinType.Default)
            {
                UpdateSkin(checkSkinType(InIHelper.ReadValue("Skin")));
            }

            var lang = InIHelper.ReadValue("Lang");
            if (string.IsNullOrEmpty(lang))
                ConfigHelper.Instance.SetLang("fa");
            else
                ConfigHelper.Instance.SetLang(lang);

            ConfigHelper.Instance.SetSystemVersionInfo(CommonHelper.GetSystemVersionInfo());

            base.OnStartup(e);
        }

        internal SkinType checkSkinType(string input)
        {
            if (input.Equals("Default"))
                return SkinType.Default;
            else if (input.Equals("Violet"))
                return SkinType.Violet;
            else
                return SkinType.Dark;
        }

        internal void UpdateSkin(SkinType skin)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin.ToString()}.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
            Current.MainWindow?.OnApplyTemplate();
        }
    }
}
using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.Win32;
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
            GlobalData.Init();

            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            //Todo: Blur Effect not work in 1903 build
            if (Convert.ToInt32(key.GetValue("ReleaseId")) < 1903)
            {
                ConfigHelper.Instance.SetSystemVersionInfo(CommonHelper.GetSystemVersionInfo());
            }

            base.OnStartup(e);
        }

        internal void UpdateSkin(SkinType skin)
        {
            ResourceDictionary skins0 = Resources.MergedDictionaries[0];
            skins0.MergedDictionaries.Clear();
            skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(skin));
            skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(typeof(App).Assembly, "Resources/Themes", skin));

            ResourceDictionary skins1 = Resources.MergedDictionaries[1];
            skins1.MergedDictionaries.Clear();
            skins1.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
            skins1.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/UrlShortener;component/Resources/Themes/Theme.xaml")
            });
            Current.MainWindow?.OnApplyTemplate();
        }
    }
}
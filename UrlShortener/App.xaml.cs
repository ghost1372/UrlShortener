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
            GlobalData.Init();

            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            ConfigHelper.Instance.SetSystemVersionInfo(CommonHelper.GetSystemVersionInfo());

            base.OnStartup(e);
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
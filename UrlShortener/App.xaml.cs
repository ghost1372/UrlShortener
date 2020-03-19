using HandyControl.Data;
using Prism.Ioc;
using System;
using System.Windows;
using UrlShortener.Views;
namespace UrlShortener
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainShortener>();
            containerRegistry.RegisterForNavigation<MainContent>();
            containerRegistry.RegisterForNavigation<LeftMainContent>();
            containerRegistry.RegisterForNavigation<About>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<Update>();
        }
        internal void UpdateSkin(SkinType skin)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin}.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }

        public override void Initialize()
        {
            base.Initialize();
            GlobalData.Init();

            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }
        }
    }
}

using HandyControl.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
namespace UrlShortener.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Change Skin and Language
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Skin))
                {
                    return;
                }

                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (GlobalData.Config.IsShowNotifyIcon)
            {
                if (GlobalData.Config.IsFirstRun)
                {
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                    {
                        Message = "Application will be hidden instead of closed and you can access it from SystemTray, do you want?",
                        Caption = "Url Shortener",
                        Button = MessageBoxButton.YesNo,
                        IconBrushKey = ResourceToken.AccentBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                    if (result == MessageBoxResult.Yes)
                    {
                        Hide();
                        e.Cancel = true;
                        GlobalData.Config.IsFirstRun = false;
                        GlobalData.Save();
                    }
                    else
                    {
                        base.OnClosing(e);
                    }
                }
                else
                {
                    Hide();
                    e.Cancel = true;
                }
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

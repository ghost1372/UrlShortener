using Prism.Commands;
using Prism.Mvvm;

namespace UrlShortener.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private bool _getIsCheckedShowNotifyIcon;
        public bool GetIsCheckedShowNotifyIcon { get => _getIsCheckedShowNotifyIcon; set => SetProperty(ref _getIsCheckedShowNotifyIcon, value); }

        public DelegateCommand<object> IsShowNotifyIconCommand { get; private set; }

        public SettingsViewModel()
        {
            IsShowNotifyIconCommand = new DelegateCommand<object>(IsShowNotifyIcon);

            InitSettings();
        }

        private void InitSettings()
        {
            GetIsCheckedShowNotifyIcon = GlobalData.Config.IsShowNotifyIcon;
        }

        private void IsShowNotifyIcon(object isChecked)
        {
            if ((bool)isChecked != GlobalData.Config.IsShowNotifyIcon)
            {
                GlobalData.Config.IsShowNotifyIcon = (bool)isChecked;
                GlobalData.Save();
            }
        }
    }
}

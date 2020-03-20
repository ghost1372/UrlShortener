using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Linq;
using System.Windows.Controls;
using UrlShortener.Views;

namespace UrlShortener.ViewModels
{
    public class LeftMainContentViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        #region Command
        public DelegateCommand<SelectionChangedEventArgs> SwitchItemCmd { get; private set; }
        public DelegateCommand UrlShortenerCmd { get; private set; }
        #endregion

        public LeftMainContentViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SwitchItemCmd = new DelegateCommand<SelectionChangedEventArgs>(Switch);
            UrlShortenerCmd = new DelegateCommand(UrlShortener);
        }

        private void UrlShortener()
        {
            object activeView = _regionManager.Regions["ContentRegion"].ActiveViews.FirstOrDefault();
            if (activeView != null)
            {
                bool IsViewMainShortener = activeView.GetType() == typeof(MainShortener);

                if (IsViewMainShortener)
                {
                    _regionManager.Regions["ContentRegion"].RemoveAll();
                }
                else
                {
                    _regionManager.RequestNavigate("ContentRegion", "MainShortener");
                }
            }
            else
            {
                _regionManager.RequestNavigate("ContentRegion", "MainShortener");
            }
        }

        private void Switch(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.AddedItems[0] is ListBoxItem item)
            {
                if (item.Tag != null)
                {
                    _regionManager.RequestNavigate("ContentRegion", item.Tag.ToString());
                }
            }

        }
    }
}

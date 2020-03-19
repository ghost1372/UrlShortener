using Prism.Mvvm;
namespace UrlShortener.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Url Shortener";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public MainWindowViewModel()
        {

        }
    }
}

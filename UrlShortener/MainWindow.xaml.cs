using HandyControl.Controls;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UrlShortener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBorderless
    {
        private string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private string BitlyLoginKey = "o_1i6m8a9v55";
        private int ServiceIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            Growl.SetGrowlPanel(PanelMessage);
        }
        public string BitlyShorten(string longUrl)
        {
            var url = string.Format("http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}", HttpUtility.UrlEncode(longUrl), BitlyLoginKey, BitlyApiKey);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic jsonResponse = js.Deserialize<dynamic>(reader.ReadToEnd());
                    string s = jsonResponse["results"][longUrl]["shortUrl"];
                    return s;
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    Growl.Error(errorText);
                }
                throw;
            }
            catch (RuntimeBinderException ex)
            {
                Growl.Error(ex.Message);
                return "";
            }
        }

        private void stck_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtCopy.Foreground = TryFindResource("SuccessBrush") as Brush;
            txtCopy.Text = "Copied";
            Clipboard.SetText(txtUrl.Text);
        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtUrl.Text))
                btn.IsEnabled = false;

            Uri uriResult;
            bool result = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
                btn.IsEnabled = true;
            else
                btn.IsEnabled = false;

            txtCopy.Text = "Click Here to Copy";
            txtCopy.Foreground = TryFindResource("BorderBrush") as Brush;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = sender as MenuItem;
            switch (tag.Tag)
            {
                case "Settings":
                    PopupWindow popup = new PopupWindow() {
                        Title = "Settings",
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        ShowInTaskbar = true
                    };

                    ComboBox cmb = new ComboBox() {
                        Style = TryFindResource("ComboBoxExtend") as Style,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    cmb.Items.Add("Bitly");
                    InfoElement.SetContentHeight(cmb, 35);
                    InfoElement.SetPlaceholder(cmb, "Choose Url Service");
                    cmb.SelectionChanged += (s, ev) => {
                        ServiceIndex = cmb.SelectedIndex;
                    };

                    StackPanel stack = new StackPanel() {
                        Margin = new Thickness(10)
                    };

                    stack.Children.Add(cmb);

                    popup.PopupElement = stack;

                    popup.ShowDialog();
                    break;

                case "Update":

                    break;

                case "About":
                    Growl.Info("Coded by Mahdi Hosseini\n Contact: mahdidvb72@gmail.com");
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (ServiceIndex)
            {
                case 0:
                    txtUrl.Text = BitlyShorten(txtUrl.Text);
                    break;
            }
            stck_MouseLeftButtonDown(null, null);
        }
    }
}
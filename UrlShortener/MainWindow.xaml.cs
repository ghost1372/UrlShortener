using HandyControl.Controls;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace UrlShortener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBorderless
    {
        public ISettings config = JsonSettings.Load<ISettings>("config.json").EnableAutosave();

        public MainWindow()
        {
            InitializeComponent();
            Growl.SetGrowlPanel(PanelMessage);
            cmbService.SelectedIndex = Convert.ToInt32(config.DefaultService);
            Topmost = config.TopMust;
        }

        public string BitlyShorten(string longUrl)
        {
            var url = string.Format("http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}", HttpUtility.UrlEncode(longUrl), config.BitlyLoginKey, config.BitlyApiKey);

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

        public string YonShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["url"] = longUrl;
                data["wish"] = customURL;

                var response = wb.UploadValues("http://api.yon.ir", "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);
                Yon result = JsonConvert.DeserializeObject<Yon>(responseInString);
                if (result.status)
                    link = "http://yon.ir/" + result.output;
                else
                    Growl.Error("that custom URL is already taken");
            }
            return link;
        }

        public class Yon
        {
            public bool status { get; set; }
            public string output { get; set; }
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

            stck.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = sender as MenuItem;
            switch (tag.Tag)
            {
                case "Settings":
                    PopupWindow popup = new PopupWindow()
                    {
                        Title = "Settings",
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        ShowInTaskbar = true
                    };

                    TextBox txtBitlyApi = new TextBox()
                    {
                        Style = TryFindResource("TextBoxExtend") as Style,
                        Width = 240,
                        Margin = new Thickness(5),
                        Text = config.BitlyApiKey
                    };
                    TextBox txtBitlyLogin = new TextBox()
                    {
                        Style = TryFindResource("TextBoxExtend") as Style,
                        Width = 240,
                        Margin = new Thickness(5),
                        Text = config.BitlyLoginKey
                    };
                    InfoElement.SetTitle(txtBitlyApi, "Change Bitly Api Key");
                    InfoElement.SetTitleAlignment(txtBitlyApi, HandyControl.Data.Enum.TitleAlignment.Top);
                    InfoElement.SetContentHeight(txtBitlyApi, 35);
                    InfoElement.SetPlaceholder(txtBitlyApi, "Bitly Api Key");
                    InfoElement.SetContentHeight(txtBitlyLogin, 35);
                    InfoElement.SetPlaceholder(txtBitlyLogin, "Bitly Login Key");

                    StackPanel stack = new StackPanel()
                    {
                        Margin = new Thickness(10)
                    };

                    CheckBox chkApi = new CheckBox()
                    {
                        Content = "Set Bitly Api or use Default",
                        IsChecked = !config.DefaultBitlyAPI
                    };
                    chkApi.Checked += (s, ev) =>
                    {
                        config.DefaultBitlyAPI = false;
                        txtBitlyApi.IsEnabled = true;
                        txtBitlyLogin.IsEnabled = true;
                    };
                    chkApi.Unchecked += (s, ev) =>
                    {
                        config.BitlyApiKey = string.Empty;
                        config.BitlyLoginKey = string.Empty;
                        config.DefaultBitlyAPI = true;
                        txtBitlyApi.IsEnabled = false;
                        txtBitlyLogin.IsEnabled = false;
                    };
                    if (config.DefaultBitlyAPI)
                    {
                        txtBitlyApi.IsEnabled = false;
                        txtBitlyLogin.IsEnabled = false;
                    }
                    else
                    {
                        txtBitlyApi.IsEnabled = true;
                        txtBitlyLogin.IsEnabled = true;
                    }

                    CheckBox chkTopMust = new CheckBox()
                    {
                        Content = "TopMust",
                        IsChecked = config.TopMust,
                        Margin = new Thickness(5)
                    };
                    chkTopMust.Checked += (s, ev) =>
                    {
                        config.TopMust = true;
                    };
                    chkTopMust.Unchecked += (s, ev) =>
                    {
                        config.TopMust = false;
                    };

                    Button btn = new Button() { Content = "Save!", Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center, Width = 140, Style = TryFindResource("ButtonPrimary") as Style };

                    btn.Click += (s, ev) =>
                    {
                        config.BitlyApiKey = txtBitlyApi.Text;
                        config.BitlyLoginKey = txtBitlyLogin.Text;
                        popup.Close();
                    };
                    stack.Children.Add(chkApi);
                    stack.Children.Add(txtBitlyApi);
                    stack.Children.Add(txtBitlyLogin);
                    stack.Children.Add(chkTopMust);
                    stack.Children.Add(btn);

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
            switch (cmbService.SelectedIndex)
            {
                case 0:
                    if (string.IsNullOrEmpty(txtCustom.Text))
                        txtUrl.Text = YonShorten(txtUrl.Text);
                    else
                        txtUrl.Text = YonShorten(txtUrl.Text, txtCustom.Text);
                    break;

                case 1:
                    txtUrl.Text = BitlyShorten(txtUrl.Text);
                    break;
            }
            Clipboard.SetText(txtUrl.Text);
            stck.Visibility = Visibility.Visible;
        }

        private void cmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            config.DefaultService = cmbService.SelectedIndex;
        }
    }
}
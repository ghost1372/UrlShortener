using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace UrlShortener
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BlurWindow
    {
        const string TopMost = nameof(TopMost);
        const string FirstRun = nameof(FirstRun);
        const string NotifyIconIsShow = nameof(NotifyIconIsShow);
        const string ServiceIndex = nameof(ServiceIndex);
        const string Skin = nameof(Skin);
        const string Lang = nameof(Lang);

        public static string getAppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static readonly HttpClient client = new HttpClient();

        private readonly List<ShorterList> shorterList = new List<ShorterList>();

        #region API Key

        private const string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private const string BitlyLoginKey = "o_1i6m8a9v55";
        private const string OpizoApiKey = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";
        private const string PlinkApiKey = "RScobms6dUhn";
        private const string MakhlasApiKey = "b64cc0ab-f274-486e-ac23-74dd3e10d9d1";
        private const string Do0ApiKey = "SJAj4Ik6Q9Rd";

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //get default values
            try
            {
                cmbListService.SelectedIndex = cmbService.SelectedIndex = Convert.ToInt32(InIHelper.ReadValue(ServiceIndex));
                tgTop.IsChecked = Topmost = Convert.ToBoolean(InIHelper.ReadValue(TopMost));
                tgNotify.IsChecked = Convert.ToBoolean(InIHelper.ReadValue(NotifyIconIsShow));

                Title = "Url Shortener " + getAppVersion;
            }
            catch (Exception)
            {
            }
        }

        #region Shorten Services

        public string BitlyShorten(string longUrl)
        {
            string url = string.Format(
                "http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}",
                HttpUtility.UrlEncode(longUrl), BitlyLoginKey, BitlyApiKey);

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
                    string errorText = reader.ReadToEnd();
                    Growl.ErrorGlobal(errorText);
                }

                throw;
            }
            catch (RuntimeBinderException ex)
            {
                Growl.ErrorGlobal(ex.Message);
                return "";
            }
        }

        public string PlinkShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
            try
            {
                using (WebClient wb = new WebClient())
                {
                    UTF8Encoding utf8 = new UTF8Encoding();
                    string utf8Encoded = HttpUtility.UrlEncode(longUrl, utf8);

                    string response = wb.DownloadString("https://plink.ir/api?api=" + PlinkApiKey + "& url=" + utf8Encoded +
                                                     "&custom=" + customURL);
                    PlinkData root = JsonConvert.DeserializeObject<PlinkData>(response);
                    if (root.error.Contains("0"))
                    {
                        link = root.@short;
                    }
                    else
                    {
                        Growl.ErrorGlobal(root.msg);
                    }
                }
            }
            catch (Exception ex)
            {

                Growl.ErrorGlobal(ex.Message);
            }

            return link;
        }


        public async Task<string> Do0Shorten(string longUrl)
        {
            string link = string.Empty;

            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>
                {
                   { "link", longUrl }
                };

                FormUrlEncodedContent content = new FormUrlEncodedContent(values);

                HttpResponseMessage response = await client.PostAsync("https://do0.ir/post/SJAj4Ik6Q9Rd/2.5", content);

                string responseString = await response.Content.ReadAsStringAsync();
                Do0Data root = JsonConvert.DeserializeObject<Do0Data>(responseString);
                if (root.success)
                {
                    link = "https://do0.ir/" + root.@short;
                }
                else
                {
                    Growl.ErrorGlobal(root.error);
                }
            }
            catch (Exception ex)
            {

                Growl.ErrorGlobal(ex.Message);
            }
            
            return link;
        }

        public string OpizoShorten(string longUrl)
        {
            string link = string.Empty;

            try
            {
                using (WebClient wb = new WebClient())
                {
                    wb.Headers.Add("X-API-KEY", OpizoApiKey);
                    NameValueCollection data = new NameValueCollection
                    {
                        ["url"] = longUrl
                    };
                    byte[] response = wb.UploadValues("https://opizo.com/api/v1/shrink/", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);

                    OpizoRootObject root = JsonConvert.DeserializeObject<OpizoRootObject>(responseInString);

                    if (root.status.Equals("success"))
                    {
                        link = root.data.url;
                    }
                    else
                    {
                        Growl.ErrorGlobal(Properties.Langs.Lang.OpizoError);
                    }
                }
            }
            catch (Exception ex)
            {

                Growl.ErrorGlobal(ex.Message);
            }

            return link;
        }

        public string YonShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
            try
            {
                using (WebClient wb = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection
                    {
                        ["url"] = longUrl,
                        ["wish"] = customURL
                    };

                    byte[] response = wb.UploadValues("http://api.yon.ir", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);
                    Yon result = JsonConvert.DeserializeObject<Yon>(responseInString);
                    if (result.status)
                    {
                        link = "http://yon.ir/" + result.output;
                    }
                    else
                    {
                        Growl.ErrorGlobal(Properties.Langs.Lang.CustomUrlError);
                    }
                }
            }
            catch (Exception ex)
            {

               Growl.ErrorGlobal(ex.Message);
            }

            return link;
        }

        #endregion

        #region Deserialize Class

        public class Do0Data
        {
            public bool success { get; set; }
            public string @short { get; set; }
            public string error { get; set; }
        }

        public class PlinkData
        {
            public string error { get; set; }
            public string @short { get; set; }
            public string msg { get; set; }
        }

        public class OpizoData
        {
            public string url { get; set; }
        }

        public class OpizoRootObject
        {
            public string status { get; set; }
            public OpizoData data { get; set; }
        }

        public class Yon
        {
            public bool status { get; set; }
            public string output { get; set; }
        }

        public class AtrabData
        {
            public string @short { get; set; }
        }

        public class AtrabRootObject
        {
            public AtrabData data { get; set; }
        }

        public class ShorterList
        {
            public string Link { get; set; }
            public string ShortLink { get; set; }
        }

        #endregion

        #region Url Shorten

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrl.Text))
            {
                btn.IsEnabled = false;
            }

            bool result = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out Uri uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                btn.IsEnabled = true;
            }
            else
            {
                btn.IsEnabled = false;
            }

            stck.Visibility = Visibility.Hidden;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbService.SelectedIndex)
            {
                case 0:
                    if (string.IsNullOrEmpty(txtCustom.Text))
                    {
                        txtUrl.Text = YonShorten(txtUrl.Text);
                    }
                    else
                    {
                        txtUrl.Text = YonShorten(txtUrl.Text, txtCustom.Text);
                    }

                    break;

                case 1:
                    txtUrl.Text = OpizoShorten(txtUrl.Text);
                    break;

                case 2:
                    txtUrl.Text = BitlyShorten(txtUrl.Text);
                    break;

                case 3:
                    if (string.IsNullOrEmpty(txtCustom.Text))
                    {
                        txtUrl.Text = PlinkShorten(txtUrl.Text);
                    }
                    else
                    {
                        txtUrl.Text = PlinkShorten(txtUrl.Text, txtCustom.Text);
                    }

                    break;
                case 4:
                    txtUrl.Text = await Do0Shorten(txtUrl.Text);
                    break;
            }

            Clipboard.SetText(txtUrl.Text);
            stck.Visibility = Visibility.Visible;
        }

        private void cmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InIHelper.AddValue(ServiceIndex, cmbService.SelectedIndex.ToString());
        }

        #endregion

        #region Group Link Shorten
        private void Button_Help(object sender, RoutedEventArgs e)
        {
            Growl.InfoGlobal(new GrowlInfo
            {
                Message =
                    Properties.Langs.Lang.Help,
                ShowDateTime = false
            });
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            shorterList.Clear();
            OpenFileDialog theDialog = new OpenFileDialog
            {
                Title = "Open Text File",
                Filter = "TXT files|*.txt"
            };
            if (theDialog.ShowDialog() == true)
            {
                string filename = theDialog.FileName;

                string[] filelines = File.ReadAllLines(filename);

                foreach (string item in filelines)
                {
                    shorterList.Add(new ShorterList { Link = item });
                }

                dataGrid.ItemsSource = shorterList;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    shorterList.Clear();
                    for (int i = 0; i < dataGrid.SelectedItems.Count; i++)
                    {
                        dynamic selectedItem = dataGrid.SelectedItems[i];
                        dynamic longLink = selectedItem.Link;
                        switch (cmbListService.SelectedIndex)
                        {
                            case 0:
                                shorterList.Add(new ShorterList { ShortLink = YonShorten(longLink) });
                                break;

                            case 1:
                                shorterList.Add(new ShorterList { ShortLink = OpizoShorten(longLink) });
                                break;

                            case 2:
                                shorterList.Add(new ShorterList { ShortLink = BitlyShorten(longLink) });
                                break;

                            case 3:
                                shorterList.Add(new ShorterList { ShortLink = PlinkShorten(longLink) });
                                break;
                            case 4:
                                shorterList.Add(new ShorterList { ShortLink = Do0Shorten(longLink) });
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                }

                SaveFileDialog oDialog = new SaveFileDialog
                {
                    Title = "Save Text File",
                    Filter = "TXT files|*.txt"
                };
                if (oDialog.ShowDialog() == true)
                {
                    File.WriteAllLines(oDialog.FileName, shorterList.Select(x => x.ShortLink));
                }

                dataGrid.ItemsSource = null;
            }), DispatcherPriority.Background);
        }

        #endregion

        #region Update

        private void CheckUpdate()
        {
            try
            {
                var checkUpdate = UpdateHelper.CheckForUpdateGithubRelease("ghost1372", "UrlShortener");

                if (checkUpdate.IsExistNewVersion)
                {
                    Growl.InfoGlobal(new GrowlInfo
                    {
                        Message = string.Format(Properties.Langs.Lang.UpdateMessage, checkUpdate.Version),
                        ShowDateTime = false,
                        ActionBeforeClose = isConfirm =>
                        {
                            if (isConfirm)
                            {
                                Process.Start(checkUpdate.Url);
                            }

                            return true;
                        },
                    });
                    Growl.InfoGlobal(checkUpdate.Changelog);
                }
                else
                {
                    Growl.ErrorGlobal(string.Format(Properties.Langs.Lang.Update, getAppVersion));
                }
            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal(ex.Message);
                
            }
        }

        #endregion

        #region Notify & Config Button
        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            popupConfig.IsOpen = true;
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                popupConfig.IsOpen = false;
                if (tag.Equals(((App)Application.Current).checkSkinType(InIHelper.ReadValue(Skin))))
                {
                    return;
                }

                InIHelper.AddValue(Skin,tag.ToString());
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                popupConfig.IsOpen = false;
                if (tag.Equals(InIHelper.ReadValue(Lang))) return;
                Growl.AskGlobal(Properties.Langs.Lang.ChangeLangAsk, b =>
                {
                    if (!b) return true;
                    InIHelper.AddValue(Lang, tag);
                    var processModule = Process.GetCurrentProcess().MainModule;
                    if (processModule != null)
                        Process.Start(processModule.FileName);
                    Application.Current.Shutdown();
                    return true;
                });
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!InIHelper.ReadValue(NotifyIconIsShow).Equals("False"))
            {
                if (!InIHelper.ReadValue(FirstRun).Equals("False"))
                {
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Properties.Langs.Lang.OnClosingMessage,
                        Caption = "Url Shotener",
                        Button = MessageBoxButton.YesNo,
                        IconBrushKey = ResourceToken.AccentBrush,
                        IconKey = ResourceToken.InfoGeometry,
                        StyleKey = "MessageBoxCustom"
                    });
                    if (result == MessageBoxResult.Yes)
                    {
                        Hide();
                        e.Cancel = true;
                        InIHelper.AddValue(FirstRun,"False");
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

        private void TgNotify_Checked(object sender, RoutedEventArgs e)
        {
            if (tgNotify.IsChecked.Value)
            {
                notify.Visibility = Visibility.Visible;
            }
            else
            {
                notify.Visibility = Visibility.Hidden;
            }

            InIHelper.AddValue(NotifyIconIsShow, tgNotify.IsChecked.Value.ToString());
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = tgTop.IsChecked.Value;
            InIHelper.AddValue(TopMost, tgTop.IsChecked.Value.ToString());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem tag = sender as MenuItem;
            switch (tag.Tag)
            {
                case "Update":
                    CheckUpdate();
                    break;

                case "About":
                    Growl.InfoGlobal(new GrowlInfo
                    { Message = Properties.Langs.Lang.AboutContent, ShowDateTime = false });
                    break;
            }
        }

        #endregion
    }
}
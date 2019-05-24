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
using System.Xml.Linq;

namespace UrlShortener
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BlurWindow
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly List<ShorterList> shorterList = new List<ShorterList>();

        public MainWindow()
        {
            InitializeComponent();

            //get default values
            try
            {
                cmbListService.SelectedIndex = cmbService.SelectedIndex = GlobalData.Config.ServiceIndex;
                tgTop.IsChecked = Topmost = GlobalData.Config.TopMost;
                tgNotify.IsChecked = GlobalData.Config.NotifyIconIsShow;

                Title = "Url Shortener " + getAppVersion;
            }
            catch (Exception)
            {
            }
        }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = tgTop.IsChecked.Value;
            GlobalData.Config.TopMost = tgTop.IsChecked.Value;
            GlobalData.Save();
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
                    Growl.Info(new GrowlInfo
                    { Message = "Coded by Mahdi Hosseini\n Contact: mahdidvb72@gmail.com", ShowDateTime = false });
                    break;
            }
        }

        private void Button_Help(object sender, RoutedEventArgs e)
        {
            Growl.Info(new GrowlInfo
            {
                Message =
                    "Step 1: Create Txt File like this(each line one Link):\n http://Test.com\nhttp://Test.com\nStep 2: Load txt File\nStep 3: Choose Url Shorten Service\nStep 4: Select urls\nStep 5: Click Start",
                ShowDateTime = false
            });
        }

        #region API Key

        private const string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private const string BitlyLoginKey = "o_1i6m8a9v55";
        private const string OpizoApiKey = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";
        private const string PlinkApiKey = "RScobms6dUhn";
        private const string MakhlasApiKey = "b64cc0ab-f274-486e-ac23-74dd3e10d9d1";
        private const string Do0ApiKey = "SJAj4Ik6Q9Rd";

        #endregion

        #region Update Variable

        private string newVersion = string.Empty;

        private string ChangeLog = string.Empty;
        private string url = "";

        public static string UpdateServer =
            "https://raw.githubusercontent.com/ghost1372/UrlShortener/master/Updater.xml";

        public const string UpdateXmlTag = "UrlShorter"; //Defined in Xml file
        public const string UpdateXmlChildTag = "AppVersion"; //Defined in Xml file
        public const string UpdateVersionTag = "version"; //Defined in Xml file
        public const string UpdateUrlTag = "url"; //Defined in Xml file
        public const string UpdateChangeLogTag = "changelog";
        public static string getAppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #endregion

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

        public string PlinkShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
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
                    Growl.Error(root.msg);
                }
            }

            return link;
        }


        public async Task<string> Do0Shorten(string longUrl)
        {
            string link = string.Empty;

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
                Growl.Error(root.error);
            }

            return link;
        }

        public string OpizoShorten(string longUrl)
        {
            string link = string.Empty;

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
                    Growl.Error("something is wrong try again");
                }
            }

            return link;
        }

        public string YonShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
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
                    Growl.Error("that custom URL is already taken");
                }
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
            GlobalData.Config.ServiceIndex = cmbService.SelectedIndex;
            GlobalData.Save();
        }

        #endregion

        #region Group Link Shorten

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
                newVersion = string.Empty;
                ChangeLog = string.Empty;
                url = "";

                XDocument doc = XDocument.Load(UpdateServer);
                IEnumerable<XElement> items = doc
                    .Element(XName.Get(UpdateXmlTag))
                    .Elements(XName.Get(UpdateXmlChildTag));
                IEnumerable<string> versionItem = items.Select(ele => ele.Element(XName.Get(UpdateVersionTag)).Value);
                IEnumerable<string> urlItem = items.Select(ele => ele.Element(XName.Get(UpdateUrlTag)).Value);
                IEnumerable<string> changelogItem = items.Select(ele => ele.Element(XName.Get(UpdateChangeLogTag)).Value);

                newVersion = versionItem.FirstOrDefault();
                url = urlItem.FirstOrDefault();
                ChangeLog = changelogItem.FirstOrDefault();
                CompareVersions();
            }
            catch (Exception)
            {
            }
        }

        private void CompareVersions()
        {
            if (IsVersionLater(newVersion, getAppVersion))
            {
                Growl.Info(new GrowlInfo
                {
                    Message = $"A new version {newVersion} has been detected!Do you want to update?",
                    ShowDateTime = false,
                    ActionBeforeClose = isConfirm =>
                    {
                        if (isConfirm)
                        {
                            Process.Start(url);
                        }

                        return true;
                    },
                    CancelStr = "Cancel",
                    ConfirmStr = "Confirm"
                });
                Growl.Info(ChangeLog);
            }
            else
            {
                Growl.Error($"You are using latest version {getAppVersion}");
            }
        }

        public static bool IsVersionLater(string newVersion, string oldVersion)
        {
            // split into groups
            string[] cur = newVersion.Split('.');
            string[] cmp = oldVersion.Split('.');
            // get max length and fill the shorter one with zeros
            int len = Math.Max(cur.Length, cmp.Length);
            int[] vs = new int[len];
            int[] cs = new int[len];
            Array.Clear(vs, 0, len);
            Array.Clear(cs, 0, len);
            int idx = 0;
            // skip non digits
            foreach (string n in cur)
            {
                if (!int.TryParse(n, out vs[idx]))
                {
                    vs[idx] = -999; // mark for skip later
                }

                idx++;
            }

            idx = 0;
            foreach (string n in cmp)
            {
                if (!int.TryParse(n, out cs[idx]))
                {
                    cs[idx] = -999; // mark for skip later
                }

                idx++;
            }

            for (int i = 0; i < len; i++)
            {
                // skip non digits
                if (vs[i] == -999 || cs[i] == -999)
                {
                    continue;
                }

                if (vs[i] < cs[i])
                {
                    return false;
                }

                if (vs[i] > cs[i])
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            popupConfig.IsOpen = true;
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                popupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Skin))
                {
                    return;
                }

                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (GlobalData.Config.NotifyIconIsShow)
            {
                HandyControl.Controls.MessageBox.Info("The tray icon is open and will hide the window instead of closing the program", "Url Shotener");
                Hide();
                e.Cancel = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private void TgNotify_Checked(object sender, RoutedEventArgs e)
        {
            GlobalData.Config.NotifyIconIsShow = tgNotify.IsChecked.Value;
            GlobalData.Save();
        }
    }
}
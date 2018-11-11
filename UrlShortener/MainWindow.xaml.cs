using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using Newtonsoft.Json;
using UrlShortener.Properties;

namespace UrlShortener
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBorderless
    {
        private readonly List<ShorterList> shorterList = new List<ShorterList>();

        public MainWindow()
        {
            InitializeComponent();

            //get default values
            cmbService.SelectedIndex = Settings.Default.Setting;
            cmbListService.SelectedIndex = Settings.Default.Setting;
            var topMost = Settings.Default.TopMost;
            tgTop.IsChecked = topMost;
            Topmost = topMost;
            Title = "Url Shorter " + getAppVersion;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tgTop.IsChecked == true)
                Settings.Default.TopMost = true;
            else
                Settings.Default.TopMost = false;

            Settings.Default.Save();
            Topmost = tgTop.IsChecked.Value;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = sender as MenuItem;
            switch (tag.Tag)
            {
                case "Update":
                    CheckUpdate();
                    break;

                case "About":
                    Growl.Info(new GrowlInfo
                        {Message = "Coded by Mahdi Hosseini\n Contact: mahdidvb72@gmail.com", ShowDateTime = false});
                    break;
            }
        }

        private void WindowBorderless_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
        }

        private void WindowBorderless_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = 600;
            if (tabc.SelectedIndex == 0)
                Height = 220;
            else
                Height = 400;
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

        //get Method
        public string AtrabIr(string longUrl)
        {
            var link = string.Empty;
            using (var wb = new WebClient())
            {
                var response = wb.DownloadString("http://s.atrab.ir/api.php?url=" + longUrl);
                var root = JsonConvert.DeserializeObject<AtrabRootObject>(response);
                link = root.data.@short;
            }

            return link;
        }

        public string BitlyShorten(string longUrl)
        {
            var url = string.Format(
                "http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}",
                HttpUtility.UrlEncode(longUrl), BitlyLoginKey, BitlyApiKey);

            var request = (HttpWebRequest) WebRequest.Create(url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var js = new JavaScriptSerializer();
                    var jsonResponse = js.Deserialize<dynamic>(reader.ReadToEnd());
                    string s = jsonResponse["results"][longUrl]["shortUrl"];
                    return s;
                }
            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response;
                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
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
            var link = string.Empty;
            using (var wb = new WebClient())
            {
                var utf8 = new UTF8Encoding();
                var utf8Encoded = HttpUtility.UrlEncode(longUrl, utf8);

                var response = wb.DownloadString("https://plink.ir/api?api=" + PlinkApiKey + "& url=" + utf8Encoded +
                                                 "&custom=" + customURL);
                var root = JsonConvert.DeserializeObject<PlinkData>(response);
                if (root.error.Contains("0"))
                    link = root.@short;
                else
                    Growl.Error(root.msg);
            }

            return link;
        }

        public string OpizoShorten(string longUrl)
        {
            var link = string.Empty;

            using (var wb = new WebClient())
            {
                wb.Headers.Add("X-API-KEY", OpizoApiKey);
                var data = new NameValueCollection();
                data["url"] = longUrl;
                var response = wb.UploadValues("https://opizo.com/api/v1/shrink/", "POST", data);
                var responseInString = Encoding.UTF8.GetString(response);

                var root = JsonConvert.DeserializeObject<OpizoRootObject>(responseInString);

                if (root.status.Equals("success"))
                    link = root.data.url;
                else
                    Growl.Error("something is wrong try again");
            }

            return link;
        }

        public string YonShorten(string longUrl, string customURL = "")
        {
            var link = string.Empty;
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["url"] = longUrl;
                data["wish"] = customURL;

                var response = wb.UploadValues("http://api.yon.ir", "POST", data);
                var responseInString = Encoding.UTF8.GetString(response);
                var result = JsonConvert.DeserializeObject<Yon>(responseInString);
                if (result.status)
                    link = "http://yon.ir/" + result.output;
                else
                    Growl.Error("that custom URL is already taken");
            }

            return link;
        }

        #endregion

        #region Deserialize Class

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
                btn.IsEnabled = false;

            Uri uriResult;
            var result = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
                btn.IsEnabled = true;
            else
                btn.IsEnabled = false;

            stck.Visibility = Visibility.Hidden;
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
                    txtUrl.Text = OpizoShorten(txtUrl.Text);
                    break;

                case 2:
                    txtUrl.Text = BitlyShorten(txtUrl.Text);
                    break;

                case 3:
                    txtUrl.Text = AtrabIr(txtUrl.Text);
                    break;

                case 4:
                    if (string.IsNullOrEmpty(txtCustom.Text))
                        txtUrl.Text = PlinkShorten(txtUrl.Text);
                    else
                        txtUrl.Text = PlinkShorten(txtUrl.Text, txtCustom.Text);
                    break;
            }

            Clipboard.SetText(txtUrl.Text);
            stck.Visibility = Visibility.Visible;
        }

        private void cmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.Setting = cmbService.SelectedIndex;
            Settings.Default.Save();
        }

        #endregion

        #region Group Link Shorten

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            shorterList.Clear();
            var theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "TXT files|*.txt";
            if (theDialog.ShowDialog() == true)
            {
                var filename = theDialog.FileName;

                var filelines = File.ReadAllLines(filename);

                foreach (var item in filelines)
                    shorterList.Add(new ShorterList {Link = item});

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
                    for (var i = 0; i < dataGrid.SelectedItems.Count; i++)
                    {
                        dynamic selectedItem = dataGrid.SelectedItems[i];
                        var longLink = selectedItem.Link;
                        switch (cmbListService.SelectedIndex)
                        {
                            case 0:
                                shorterList.Add(new ShorterList {ShortLink = YonShorten(longLink)});
                                break;

                            case 1:
                                shorterList.Add(new ShorterList {ShortLink = OpizoShorten(longLink)});
                                break;

                            case 2:
                                shorterList.Add(new ShorterList {ShortLink = BitlyShorten(longLink)});
                                break;

                            case 3:
                                shorterList.Add(new ShorterList {ShortLink = AtrabIr(longLink)});
                                break;
                            case 4:
                                shorterList.Add(new ShorterList {ShortLink = PlinkShorten(longLink)});
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                }

                var oDialog = new SaveFileDialog();
                oDialog.Title = "Save Text File";
                oDialog.Filter = "TXT files|*.txt";
                if (oDialog.ShowDialog() == true)
                    File.WriteAllLines(oDialog.FileName, shorterList.Select(x => x.ShortLink));
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

                var doc = XDocument.Load(UpdateServer);
                var items = doc
                    .Element(XName.Get(UpdateXmlTag))
                    .Elements(XName.Get(UpdateXmlChildTag));
                var versionItem = items.Select(ele => ele.Element(XName.Get(UpdateVersionTag)).Value);
                var urlItem = items.Select(ele => ele.Element(XName.Get(UpdateUrlTag)).Value);
                var changelogItem = items.Select(ele => ele.Element(XName.Get(UpdateChangeLogTag)).Value);

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
                            Process.Start(url);

                        return true;
                    },
                    CancelStr = "Cancel", ConfirmStr = "Confirm"
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
            var cur = newVersion.Split('.');
            var cmp = oldVersion.Split('.');
            // get max length and fill the shorter one with zeros
            var len = Math.Max(cur.Length, cmp.Length);
            var vs = new int[len];
            var cs = new int[len];
            Array.Clear(vs, 0, len);
            Array.Clear(cs, 0, len);
            var idx = 0;
            // skip non digits
            foreach (var n in cur)
            {
                if (!int.TryParse(n, out vs[idx])) vs[idx] = -999; // mark for skip later
                idx++;
            }

            idx = 0;
            foreach (var n in cmp)
            {
                if (!int.TryParse(n, out cs[idx])) cs[idx] = -999; // mark for skip later
                idx++;
            }

            for (var i = 0; i < len; i++)
            {
                // skip non digits
                if (vs[i] == -999 || cs[i] == -999) continue;
                if (vs[i] < cs[i])
                    return false;
                if (vs[i] > cs[i]) return true;
            }

            return false;
        }

        #endregion
    }
}
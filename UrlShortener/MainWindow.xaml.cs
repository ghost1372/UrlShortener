using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
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

namespace UrlShortener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBorderless
    {
        #region API Key
        private const string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private const string BitlyLoginKey = "o_1i6m8a9v55";
        private const string OpizoApiKey = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";
        private const string PlinkApiKey = "RScobms6dUhn";

        #endregion
        #region Update Variable
        private string newVersion = string.Empty;

        private string ChangeLog = string.Empty;
        private string url = "";
        public static string UpdateServer = "https://raw.githubusercontent.com/ghost1372/UrlShortener/master/Updater.xml";
        public const string UpdateXmlTag = "UrlShorter"; //Defined in Xml file
        public const string UpdateXmlChildTag = "AppVersion"; //Defined in Xml file
        public const string UpdateVersionTag = "version"; //Defined in Xml file
        public const string UpdateUrlTag = "url"; //Defined in Xml file
        public const string UpdateChangeLogTag = "changelog";
        public static string getAppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        #endregion

        private List<ShorterList> shorterList = new List<ShorterList>();

        public MainWindow()
        {
            InitializeComponent();
            Growl.SetGrowlPanel(PanelMessage);

            //get default values
            cmbService.SelectedIndex = Properties.Settings.Default.Setting;
            cmbListService.SelectedIndex = Properties.Settings.Default.Setting;
            var topMost = Properties.Settings.Default.TopMost;
            tgTop.IsChecked = topMost;
            this.Topmost = topMost;
        }

        #region Shorten Services

        //get Method
        public string AtrabIr(string longUrl)
        {
            string link = string.Empty;
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

        public string PlinkShorten(string longUrl, string customURL = "")
        {
            string link = string.Empty;
            using (var wb = new WebClient())
            {
                var response = wb.DownloadString("https://plink.ir/api?api=" + PlinkApiKey + "& url="+ longUrl + "&custom=" + customURL);
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
            string link = string.Empty;

            using (var wb = new WebClient())
            {
                wb.Headers.Add("X-API-KEY", OpizoApiKey);
                var data = new NameValueCollection();
                data["url"] = longUrl;
                var response = wb.UploadValues("https://opizo.com/api/v1/shrink/", "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

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
            bool result = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out uriResult)
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
            Properties.Settings.Default.Setting = cmbService.SelectedIndex;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Group Link Shorten
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            shorterList.Clear();
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "TXT files|*.txt";
            if (theDialog.ShowDialog() == true)
            {
                string filename = theDialog.FileName;

                string[] filelines = File.ReadAllLines(filename);

                foreach (var item in filelines)
                    shorterList.Add(new ShorterList { Link = item });

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
                        var longLink = selectedItem.Link;
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
                                shorterList.Add(new ShorterList { ShortLink = AtrabIr(longLink) });
                                break;
                            case 4:
                                shorterList.Add(new ShorterList { ShortLink = PlinkShorten(longLink) });
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
                    System.IO.File.WriteAllLines(oDialog.FileName, shorterList.Select(x => x.ShortLink));
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
            if (IsVersionLater(newVersion, getAppVersion.ToString()))
            {
                Growl.Info(new GrowlInfo
                {
                    Message = $"A new version {newVersion} has been detected!Do you want to update?",
                    ShowDateTime = false,
                    ActionBeforeClose = isConfirm =>
                    {
                        if (isConfirm)
                            System.Diagnostics.Process.Start(url);

                        return true;
                    }
                });
                Growl.Info(ChangeLog);
            }
            else
            {
                Growl.Error($"You are using latest version {getAppVersion.ToString()}");
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
                if (!Int32.TryParse(n, out vs[idx]))
                {
                    vs[idx] = -999; // mark for skip later
                }
                idx++;
            }
            idx = 0;
            foreach (string n in cmp)
            {
                if (!Int32.TryParse(n, out cs[idx]))
                {
                    cs[idx] = -999; // mark for skip later
                }
                idx++;
            }
            for (int i = 0; i < len; i++)
            {
                // skip non digits
                if ((vs[i] == -999) || (cs[i] == -999))
                {
                    continue;
                }
                if (vs[i] < cs[i])
                {
                    return (false);
                }
                else if (vs[i] > cs[i])
                {
                    return (true);
                }
            }
            return (false);
        }

        #endregion

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tgTop.IsChecked == true)
                Properties.Settings.Default.TopMost = true;
            else
                Properties.Settings.Default.TopMost = false;

            Properties.Settings.Default.Save();
            this.Topmost = tgTop.IsChecked.Value;
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
                    Growl.Info("Coded by Mahdi Hosseini\n Contact: mahdidvb72@gmail.com");
                    break;
            }
        }
        private void WindowBorderless_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void WindowBorderless_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = 600;
            if (tabc.SelectedIndex == 0)
                this.Height = 220;
            else
                this.Height = 400;
        }
    }
}
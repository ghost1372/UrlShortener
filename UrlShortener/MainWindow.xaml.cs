using HandyControl.Controls;
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
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace UrlShortener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBorderless
    {
        private const string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private const string BitlyLoginKey = "o_1i6m8a9v55";
        private const string OpizoApiKey = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";

        public MainWindow()
        {
            InitializeComponent();
            Growl.SetGrowlPanel(PanelMessage);
            cmbService.SelectedIndex = Properties.Settings.Default.Setting;
        }

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
                    txtUrl.Text = OpizoShorten(txtUrl.Text);
                    break;

                case 2:
                    txtUrl.Text = BitlyShorten(txtUrl.Text);
                    break;

                case 3:
                    txtUrl.Text = AtrabIr(txtUrl.Text);
                    break;

                case 4:
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

        public class ShorterList
        {
            public string Link { get; set; }
            public string ShortLink { get; set; }
        }

        private List<ShorterList> shorterList = new List<ShorterList>();

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
    }
}
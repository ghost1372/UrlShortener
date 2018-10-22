using HandyControl.Controls;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.ComponentModel;
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
    public partial class MainWindow : WindowBorderless, INotifyPropertyChanged
    {
        public int _Index { get; set; } = 0;
        private int LastIndex = 2;
        private string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private string BitlyLoginKey = "o_1i6m8a9v55";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            Growl.SetGrowlPanel(PanelMessage);
            DataContext = this;
        }

        public int Index
        {
            get { return _Index; }
            set { _Index = value; OnPropertyChanged("Index"); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
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
            Clipboard.SetText(txtShort.Text);
        }

        private void StepBar_Click(object sender, RoutedEventArgs e)
        {
            var tag = sender as Button;
            if (tag.Tag.Equals("Next"))
            {
                if (Index == LastIndex)
                    return;
                if (Index < LastIndex)
                    Index++;
            }
            else
            {
                if (Index == 0)
                    return;

                Index--;
            }
            if (Index == 1)
                btnNext.IsEnabled = false;

            if (Index == 2)
            {
                switch (cmbService.SelectedIndex)
                {
                    case 0:
                        txtShort.Text = BitlyShorten(txtUrl.Text);
                        break;
                }
                txtCopy.Text = "Click Here to Copy";
                txtCopy.Foreground = TryFindResource("BorderBrush") as Brush;
            }
        }

        private void txtShort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(txtShort.Text);
        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtUrl.Text))
                btnNext.IsEnabled = false;
                
            Uri uriResult;
            bool result = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (result)
                btnNext.IsEnabled = true;
            else
                btnNext.IsEnabled = false;
        }

        private void cmbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnNext.IsEnabled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Growl.Info("Coded by Mahdi Hosseini\n Contact: mahdidvb72@gmail.com");
        }
    }
}
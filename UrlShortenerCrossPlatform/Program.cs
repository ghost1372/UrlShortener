using CommandLine;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using UrlShortenerCrossPlatform.ClipboardAPI;

namespace UrlShortenerCrossPlatform
{
    class Program
    {
        private static string currentLink = string.Empty;
        #region API Key

        private const string BitlyApiKey = "R_c597e397b606436fa6a9179626da61bb";
        private const string BitlyLoginKey = "o_1i6m8a9v55";
        private const string OpizoApiKey = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";
        private const string PlinkApiKey = "RScobms6dUhn";
        private const string MakhlasApiKey = "b64cc0ab-f274-486e-ac23-74dd3e10d9d1";
        private const string Do0ApiKey = "SJAj4Ik6Q9Rd";

        #endregion
        #region Shorten Services

        //get Method
        public static string AtrabIr(string longUrl)
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

        public static string BitlyShorten(string longUrl)
        {
            var url = string.Format(
                "http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={0}&login={1}&apiKey={2}",
                HttpUtility.UrlEncode(longUrl), BitlyLoginKey, BitlyApiKey);

            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
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
                    Console.WriteLine(errorText);
                }

                throw;
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static string PlinkShorten(string longUrl, string customURL = "")
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
                    Console.WriteLine(root.msg);
            }

            return link;
        }

        public static string Do0Shorten(string longUrl)
        {
            var link = string.Empty;

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["link"] = longUrl;
                var response = wb.UploadValues("https://do0.ir/post/SJAj4Ik6Q9Rd/2.5", "POST", data);
                var responseInString = Encoding.UTF8.GetString(response);

                var root = JsonConvert.DeserializeObject<Do0Data>(responseInString);

                if (root.success)
                    link = root.@short;
                else
                    Console.WriteLine(root.error);
            }

            return "https://do0.ir/" + link;
        }

        public static string OpizoShorten(string longUrl)
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
                    Console.WriteLine("something is wrong try again");
            }

            return link;
        }

        public static string YonShorten(string longUrl, string customURL = "")
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
                    Console.WriteLine("that custom URL is already taken");
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

        public static string NumberToString(int value)
        {
            switch (value)
            {
                case 1:
                    return "Yon";

                case 2:
                    return "Opizo";

                case 3:
                    return "Bitly";

                case 4:
                    return "Atrab";
                case 5:
                    return "Plink";
                case 6:
                    return "Do0";

                default:
                    return "Opizo";
            }
        }
        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Coded by Mahdi Hosseini");
                    Console.WriteLine("Email: mahdidvb72@gmail.com");
                    Console.WriteLine("Github: ghost1372");
                    Console.WriteLine();
                    Console.WriteLine();
                    if (!string.IsNullOrEmpty(o.Link))
                    {
                        
                        Uri uriResult;
                        var result = Uri.TryCreate(o.Link, UriKind.Absolute, out uriResult)
                                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                        if (result)
                        {
                            Console.WriteLine($"{NumberToString(o.Service)} selected.");
                            Console.WriteLine("we are sending your request please w8...");
                            switch (o.Service)
                            {
                                case 1:
                                    if (string.IsNullOrEmpty(o.Custom))
                                        currentLink = YonShorten(o.Link);
                                    else
                                        currentLink = YonShorten(o.Link, o.Custom);
                                    break;

                                case 2:
                                    currentLink = OpizoShorten(o.Link);
                                    break;

                                case 3:
                                    currentLink = BitlyShorten(o.Link);
                                    break;

                                case 4:
                                    currentLink = AtrabIr(o.Link);
                                    break;

                                case 5:
                                    if (string.IsNullOrEmpty(o.Custom))
                                        currentLink = PlinkShorten(o.Link);
                                    else
                                        currentLink = PlinkShorten(o.Link, o.Custom);
                                    break;
                                case 6:
                                    currentLink = Do0Shorten(o.Link);
                                    break;
                                default:
                                    currentLink = OpizoShorten(o.Link);
                                    break;
                            }

                            Clipboard.Copy(currentLink);
                            Console.WriteLine($"your Short link is: {currentLink}");
                            Console.WriteLine("your link copied to clipboard");
                            Console.WriteLine("Done!");

                            Console.ForegroundColor = ConsoleColor.Cyan;

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("enter valid url");
                            Console.ForegroundColor = ConsoleColor.Cyan;

                        }


                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("you must set long url Arguments");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                });
        }
        
    }
}

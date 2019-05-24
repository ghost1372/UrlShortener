using CommandLine;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using UrlShortenerCrossPlatform.ClipboardAPI;

namespace UrlShortenerCrossPlatform
{
    internal class Program
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
            string link = string.Empty;
            using (WebClient wb = new WebClient())
            {
                string response = wb.DownloadString("http://s.atrab.ir/api.php?url=" + longUrl);
                AtrabRootObject root = JsonConvert.DeserializeObject<AtrabRootObject>(response);
                link = root.data.@short;
            }

            return link;
        }

        public static string BitlyShorten(string longUrl)
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
                    dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
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
                    Console.WriteLine(root.msg);
                }
            }

            return link;
        }

        public static string Do0Shorten(string longUrl)
        {
            string link = string.Empty;

            using (WebClient wb = new WebClient())
            {
                NameValueCollection data = new NameValueCollection
                {
                    ["link"] = longUrl
                };
                byte[] response = wb.UploadValues("https://do0.ir/post/SJAj4Ik6Q9Rd/2.5", "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                Do0Data root = JsonConvert.DeserializeObject<Do0Data>(responseInString);

                if (root.success)
                {
                    link = root.@short;
                }
                else
                {
                    Console.WriteLine(root.error);
                }
            }

            return "https://do0.ir/" + link;
        }

        public static string OpizoShorten(string longUrl)
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
                    Console.WriteLine("something is wrong try again");
                }
            }

            return link;
        }

        public static string YonShorten(string longUrl, string customURL = "")
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
                    Console.WriteLine("that custom URL is already taken");
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

        private static void Main(string[] args)
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

                        bool result = Uri.TryCreate(o.Link, UriKind.Absolute, out Uri uriResult)
                                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                        if (result)
                        {
                            Console.WriteLine($"{NumberToString(o.Service)} selected.");
                            Console.WriteLine("we are sending your request please w8...");
                            switch (o.Service)
                            {
                                case 1:
                                    if (string.IsNullOrEmpty(o.Custom))
                                    {
                                        currentLink = YonShorten(o.Link);
                                    }
                                    else
                                    {
                                        currentLink = YonShorten(o.Link, o.Custom);
                                    }

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
                                    {
                                        currentLink = PlinkShorten(o.Link);
                                    }
                                    else
                                    {
                                        currentLink = PlinkShorten(o.Link, o.Custom);
                                    }

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

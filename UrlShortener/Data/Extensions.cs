using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace UrlShortener.Data
{
    public static class Extensions
    {
        public static StringContent AsJson(this object o)
        {
            return new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
        }
    }
}

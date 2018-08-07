using System.Net.Http;

// JsonContent Class packages
using Newtonsoft.Json;
using System.Text;

namespace alarms
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }
}

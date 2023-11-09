using Newtonsoft.Json;

namespace BaseProject.DAO.Models.API.OpenAI
{
    public class ChatResponse
    {
        public string id { get; set; }
        [JsonProperty("object")]
        public string obj { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
        public Choice[] choices { get; set; }
    }

}

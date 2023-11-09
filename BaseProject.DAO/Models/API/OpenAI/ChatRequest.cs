namespace BaseProject.DAO.Models.API.OpenAI
{
    public class ChatRequest
    {
        public Message[] messages { get; set; }
        public decimal? temperature { get; set; }
        public int? max_tokens { get; set; }
        public string user { get; set; }
    }
}

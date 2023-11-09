namespace BaseProject.DAO.Models.API.OpenAI
{
    public class Choice
    {
        public Message message { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
    }

}

namespace BaseProject.DAO.Models.Views
{
	public class ArquivoVM
	{
		public string Nome { get; set; }
		public string Extensao { get; set; }
		public int Tamanho { get; set; }
        public string Tipo { get; set; }
        public string Base64 { get; set; }

        public ArquivoVM() { }
    }
}
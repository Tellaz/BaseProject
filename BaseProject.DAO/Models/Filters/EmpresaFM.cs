
namespace BaseProject.DAO.Models.Filters
{
	public class EmpresaFM
	{		
		public string RazaoSocial { get; set; }
		public DateTime? DataCadastroInicio { get; set; }
		public DateTime? DataCadastroFim { get; set; }
		public DateTime? DataCadastro { get; set; }
        public byte? TipoPlano { get; set; }
		public bool? Ativa { get; set; }
	}
}

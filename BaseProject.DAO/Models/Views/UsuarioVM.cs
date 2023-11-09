
using BaseProject.Util;
using System.Text;

namespace BaseProject.DAO.Models.Views
{
    public class UsuarioVM
    {
		public int Id { get; set; }
		public string IdAspNetUser { get; set; }
		public int IdEmpresa { get; set; }
		public string Nome { get; set; }
		public string Sobrenome { get; set; }
		public string NomeCompleto { get; set; }
		public string Email { get; set; }
		public string CPF { get; set; }
        public string Senha { get; set; }
		public string DataCadastro { get; set; }
		public bool Ativo { get; set; }  
        public AspNetUser IdAspNetUserNavigation { get; set; }
        public Empresa IdEmpresaNavigation { get; set; }
        public string NomeEmpresa { get; set; }
        public UsuarioFoto UsuarioFoto { get; set; }
        public string FotoDataUrl { get; set; }

        public UsuarioVM() { }

        public UsuarioVM(Usuario model)
        {
            Id = model.Id;
            IdAspNetUser = model.IdAspNetUser;
            IdEmpresa = model.IdEmpresa;
            Nome = model.Nome;
            Sobrenome = model.Sobrenome;
            NomeCompleto = $"{model.Nome}{(string.IsNullOrEmpty(model.Sobrenome) ? "" : $" {model.Sobrenome}")}";
			Email = model.Email;
            CPF = string.IsNullOrEmpty(model.CPF) ? "" : model.CPF.MaskCPF();
            Senha = Encoding.UTF8.GetString(Convert.FromBase64String(model.Senha));
            DataCadastro = model.DataCadastro.ToString("dd/MM/yyyy");
            Ativo = model.Ativo;
            IdAspNetUserNavigation = model.IdAspNetUserNavigation;
            IdEmpresaNavigation = model.IdEmpresaNavigation;
            NomeEmpresa = model.IdEmpresaNavigation != null ? model.IdEmpresaNavigation.RazaoSocial : "";            
            UsuarioFoto = model.UsuarioFoto;
            FotoDataUrl = model.UsuarioFoto != null ? ("data:" + model.UsuarioFoto.Tipo + ";base64," + model.UsuarioFoto.Base64) : "";            
        }

    }
}
using BaseProject.Util;
using System.Text;

namespace BaseProject.DAO.Models.Views.Account
{
    public class UserVM
    {
        public int idUsuario { get; set; }
        public string idAspNetUser { get; set; }
        public int idEmpresa { get; set; }
        public int? idRepresentante { get; set; }
        public string nome { get; set; }
        public string sobrenome { get; set; }
		public string nomeCompleto { get; set; }
		public string email { get; set; }
        public string cpf { get; set; }
        public string dataCadastro { get; set; }
        public bool ativo { get; set; }
        public string nomeEmpresa { get; set; }
        public string logoDataUrl { get; set; }
        public string fotoDataUrl { get; set; }
        public bool primeiroAcesso { get; set; }
        public bool twoFactorEnabled { get; set; }
        public string telefone { get; set; }        

        public UserVM() { }

        public UserVM(AspNetUser user)
        {
            idUsuario = user.Usuario.Id;
            idAspNetUser = user.Id;
            idEmpresa = user.Usuario.IdEmpresa;
            idRepresentante = user.Usuario.IdEmpresaNavigation != null ? user.Usuario.IdEmpresaNavigation.IdRepresentante : null;
            nome = user.Usuario.Nome;
			sobrenome = user.Usuario.Sobrenome ?? "";
			nomeCompleto = $"{user.Usuario.Nome}{(string.IsNullOrEmpty(user.Usuario.Sobrenome) ? "" : $" {user.Usuario.Sobrenome}")}";
			email = user.Usuario.Email;
            cpf = string.IsNullOrEmpty(user.Usuario.CPF) ? "" : user.Usuario.CPF.MaskCPF();
            dataCadastro = user.Usuario.DataCadastro.ToString("dd/MM/yyyy");
            ativo = user.Usuario.Ativo;
            nomeEmpresa = user.Usuario.IdEmpresaNavigation != null ? user.Usuario.IdEmpresaNavigation.RazaoSocial : "";
            logoDataUrl = user.Usuario.IdEmpresaNavigation != null && user.Usuario.IdEmpresaNavigation.EmpresaLogo != null ? "data:" + user.Usuario.IdEmpresaNavigation.EmpresaLogo.Tipo + ";base64," + user.Usuario.IdEmpresaNavigation.EmpresaLogo.Base64 : "";
            fotoDataUrl = user.Usuario.UsuarioFoto != null ? "data:" + user.Usuario.UsuarioFoto.Tipo + ";base64," + user.Usuario.UsuarioFoto.Base64 : "";
            primeiroAcesso = user.Usuario.PrimeiroAcesso;
            twoFactorEnabled = user.TwoFactorEnabled;
            telefone = string.IsNullOrEmpty(user.PhoneNumber) ? "" : user.PhoneNumber.MaskTelefone();
        }

    }
}
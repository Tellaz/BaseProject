
using BaseProject.DAO.Models.Views.Account;
using BaseProject.Util;
using System.ComponentModel.DataAnnotations;

namespace BaseProject.DAO.Models.Views
{
    public class EmpresaVM
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string CNPJ { get; set; }
        public string DataCadastro { get; set; }
        public bool Ativa { get; set; }
        public int? IdRepresentante { get; set; }
        public string Dominio { get; set; }        
        public EmpresaLogo EmpresaLogo { get; set; }
        public string LogoDataUrl { get; set; }

        public EmpresaVM() { }

        public EmpresaVM(Empresa model)
        {
            Id = model.Id;
            RazaoSocial = model.RazaoSocial ?? "";
            NomeFantasia = model.NomeFantasia ?? "";
            CNPJ = model.CNPJ.MaskCNPJ() ?? "";
            DataCadastro = model.DataCadastro.ToString("dd/MM/yyyy") ?? "";
            Ativa = model.Ativa;
            IdRepresentante = model.IdRepresentante;
            Dominio = model.Dominio;                    
            EmpresaLogo = model.EmpresaLogo;
            LogoDataUrl = model.EmpresaLogo != null ? ("data:" + model.EmpresaLogo.Tipo + ";base64," + model.EmpresaLogo.Base64) : "";            
        }

    }
}
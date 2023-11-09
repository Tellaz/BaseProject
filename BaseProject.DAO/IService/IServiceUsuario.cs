using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using static BaseProject.DAO.Service.ServiceUsuario;

namespace BaseProject.DAO.IService
{
	public interface IServiceUsuario : IService<Usuario>
    {
        Usuario ObterPorIdAspNetUser(string idAspNetUser, string includeProperties = "");
        Usuario[] ObterPorIdEmpresa(int idEmpresa, string includeProperties = "");
        Usuario ObterPorEmail(string email, string includeProperties = "");
        bool IsRepresentante(int idUsuario);
        bool EditarSenha(int id, string senha);
        bool Existe(string email, string cpf);
        bool ExisteEmail(string email);
        bool ExisteCPF(string cpf);
        DTResult<UsuarioVM> Listar(DTParam<UsuarioFM> param, int idEmpresa);
        int ObterQtdUsuarioPorEmpresa(int idEmpresa);
		UsuarioVM[] ObterParaSelect(int idEmpresaUsuario, string includeProperties = "", bool noTracking = true);
        int? ObterIdEmpresaSelecionada(int idUsuario);
        int ObterIdEmpresaSelecionada(HttpContext httpContext);	
	}
}

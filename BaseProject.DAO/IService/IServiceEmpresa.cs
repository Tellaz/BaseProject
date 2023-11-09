using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models.Views.Account;

namespace BaseProject.DAO.IService
{
	public interface IServiceEmpresa : IService<Empresa>
    {
        Empresa ObterPorDominio(string dominio, string includeProperties = "");
        Empresa[] ObterPorIdRepresentante(int idRepresentante, string includeProperties = "");
        EmpresaVM[] ObterParaSelect(bool? ativa = null);
        bool Existe(string dominio);
        DTResult<EmpresaVM> Listar(DTParam<EmpresaFM> param);
        S2Result ListarSelect2(S2Param param);
    }
}

using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;

namespace BaseProject.DAO.IService
{
	public interface IServiceAspNetUser : IService<AspNetUser>
    {
        AspNetUser ObterPorIdAspNetUser(string idAspNetUser, string includeProperties = "");
        AspNetUser[] ObterAtivosPorIdEmpresa(int idEmpresa);
        AspNetUser ObterPorEmail(string email, string includeProperties = "");
        AspNetUser ObterPorUserName(string userName, string includeProperties = "");
    }
}

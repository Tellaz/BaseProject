using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;

namespace BaseProject.DAO.IService
{
	public interface IServiceDownload : IService<Download>
    {
        bool Processando(int idEmpresa, byte? tipo = null);
        DTResult<DownloadVM> Listar(DTParam<DownloadFM> param, int idEmpresa);
    }
}

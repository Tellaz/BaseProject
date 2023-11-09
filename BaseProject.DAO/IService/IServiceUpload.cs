using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;

namespace BaseProject.DAO.IService
{
	public interface IServiceUpload : IService<Upload>
    {
        bool Processando(int idEmpresa, byte? tipo = null);
        DTResult<UploadVM> Listar(DTParam<UploadFM> param, int idEmpresa);
    }
}

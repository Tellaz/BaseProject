using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaseProject.DAO.IRepository;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Repository;
using BaseProject.Util;
using System.Text;

namespace BaseProject.DAO.Service
{
	public class ServiceAspNetUser : IServiceAspNetUser
    {
        private readonly IRepositoryAspNetUser _repositoryAspNetUser;

        public ServiceAspNetUser(IRepositoryAspNetUser repositoryAspNetUser)
        {
            _repositoryAspNetUser = repositoryAspNetUser;
        }

        public AspNetUser ObterPorId(int id, string includeProperties = "")
        {
            return _repositoryAspNetUser.FirstOrDefault(x => x.Usuario.Id == id, includeProperties);
        }

        public AspNetUser[] ObterTodos(string includeProperties = "", bool noTracking = true)
        {
            return _repositoryAspNetUser.Get(includeProperties: includeProperties, noTracking: noTracking);
        }

        public AspNetUser ObterPorIdAspNetUser(string idAspNetUser, string includeProperties = "")
        {
            return _repositoryAspNetUser.FirstOrDefault(x => x.Id == idAspNetUser, includeProperties: includeProperties);
        }

        public AspNetUser[] ObterAtivosPorIdEmpresa(int idEmpresa)
        {
            return _repositoryAspNetUser.Get(x => x.Usuario.IdEmpresa == idEmpresa && x.Usuario.Ativo, includeProperties: "Usuario");
        }

        public AspNetUser ObterPorEmail(string email, string includeProperties = "")
        {
            return _repositoryAspNetUser.FirstOrDefault(x => x.Email.ToLower() == email.ToLower(), includeProperties: includeProperties);
        }

        public AspNetUser ObterPorUserName(string userName, string includeProperties = "")
        {
            return _repositoryAspNetUser.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower(), includeProperties: includeProperties);
        }

        public bool Adicionar(AspNetUser entity)
        {
            return _repositoryAspNetUser.Insert(entity);
        }

        public bool Editar(AspNetUser entity)
        {
            return _repositoryAspNetUser.Update(entity);
        }

        public bool Deletar(int id)
        {
            return _repositoryAspNetUser.Delete(id);
        }

    }
}

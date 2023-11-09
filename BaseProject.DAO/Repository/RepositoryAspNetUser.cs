using BaseProject.DAO.Data;
using BaseProject.DAO.IRepository;
using BaseProject.DAO.Models;

namespace BaseProject.DAO.Repository
{
	public class RepositoryAspNetUser : Repository<AspNetUser, ApplicationDbContext>, IRepositoryAspNetUser
    {
        public RepositoryAspNetUser() : base() { }

        public RepositoryAspNetUser(ApplicationDbContext context) : base(context) { }

    }
}

using BaseProject.DAO.Data;
using BaseProject.DAO.IRepository;
using BaseProject.DAO.Models;

namespace BaseProject.DAO.Repository
{
	public class RepositoryUpload : Repository<Upload, ApplicationDbContext>, IRepositoryUpload
    {
        public RepositoryUpload() : base() { }

        public RepositoryUpload(ApplicationDbContext context) : base(context) { }

    }
}

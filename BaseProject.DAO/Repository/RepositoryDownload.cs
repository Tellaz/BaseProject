using BaseProject.DAO.Data;
using BaseProject.DAO.IRepository;
using BaseProject.DAO.Models;

namespace BaseProject.DAO.Repository
{
	public class RepositoryDownload : Repository<Download, ApplicationDbContext>, IRepositoryDownload
    {
        public RepositoryDownload() : base() { }

        public RepositoryDownload(ApplicationDbContext context) : base(context) { }

    }
}

using BaseProject.DAO.IRepository;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Repository;
using BaseProject.Util;
using System.Linq.Expressions;

namespace BaseProject.DAO.Service
{
	public class ServiceUpload : IServiceUpload
    {
        private readonly IRepositoryUpload _repositoryUpload;

        public ServiceUpload(IRepositoryUpload repositoryUpload)
        {
            _repositoryUpload = repositoryUpload;
        }

        public Upload ObterPorId(int id, string includeProperties = "")
        {
            return _repositoryUpload.FirstOrDefault(x => x.Id == id, includeProperties);
        }

        public Upload[] ObterTodos(string includeProperties = "", bool noTracking = true)
        {
            return _repositoryUpload.Get(includeProperties: includeProperties, noTracking: noTracking);
        }

        public bool Processando(int idEmpresa, byte? tipo = null)
        {
            if (tipo.HasValue) return _repositoryUpload.Exists(x => x.IdEmpresa == idEmpresa && x.Tipo == tipo.Value && x.Status == (byte)EnumTaskStatus.Processando);

            return _repositoryUpload.Exists(x => x.IdEmpresa == idEmpresa && x.Status == (byte)EnumTaskStatus.Processando);
        }

        public bool Adicionar(Upload entity)
        {
            return _repositoryUpload.Insert(entity);
        }

        public bool Editar(Upload entity)
        {
            return _repositoryUpload.Update(entity);
        }

        public bool Deletar(int id)
        {
            return _repositoryUpload.Delete(id);
        }

        public DTResult<UploadVM> Listar(DTParam<UploadFM> param, int idEmpresa)
        {
            var query = _repositoryUpload.GetContext().Set<Upload>().AsQueryable();

            query = query.Where(x => x.IdEmpresa == idEmpresa);

            //Adicione as colunas de texto onde a pesquisa geral será aplicada
            var search = param.SearchValue();
            if (!string.IsNullOrEmpty(search)) query = query.Where(x =>
                x.MD5.Contains(search)
            );

            //Adicione as colunas ordenáveis e o seu respectivo nome do datatables (É necessário pelo menos uma como padrão)
            var keyGen = new KeySelectorGenerator<Upload>(param.SortedColumnName());
            keyGen.AddKeySelector(x => x.MD5, "MD5");
            keyGen.AddKeySelector(x => x.Tipo, "TipoString");
            keyGen.AddKeySelector(x => x.Status, "StatusString");
            keyGen.AddKeySelector(x => x.DataInicial, "DataInicialString");
            keyGen.AddKeySelector(x => x.DataFinal.Value, "DataFinalString");
            query = keyGen.Sort(query, param.IsAscendingSort());

            //Adicione os filtros avançados
            var filters = param.Filters;
            if (filters is not null)
            {
                if (filters.Tipo.HasValue) query = query.Where(x => x.Tipo == filters.Tipo.Value);
                if (filters.Status.HasValue) query = query.Where(x => x.Status == filters.Status.Value);
                if (filters.DataInicial.HasValue) query = query.Where(x => x.DataInicial == filters.DataInicial.Value);
                if (filters.DataFinal.HasValue) query = query.Where(x => x.DataFinal.HasValue && x.DataFinal.Value == filters.DataFinal.Value);
            }

            //Adicione as tabelas relacionadas caso precise
            var includeProperties = "";

            var itens = _repositoryUpload.Filter(query, param.InitialPosition(), param.ItensPerPage(), out int total, includeProperties);

            return new DTResult<UploadVM>
            {
                Itens = itens.Select(x => new UploadVM(x)).ToArray(),
                Total = total
            };
        }

    }
}

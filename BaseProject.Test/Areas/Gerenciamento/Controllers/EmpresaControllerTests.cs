using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using BaseProject.DAO.Data;
using BaseProject.DAO.IService;
using BaseProject.DAO.Repository;
using BaseProject.DAO.Service;
using BaseProject.Test.Mocks;
using BaseProject.Util;
using Xunit;
using BaseProject.API.Areas.Gerenciamento.Controllers;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models;

namespace BaseProject.Test.Areas.Gerenciamento.Controllers
{
    public class EmpresaControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<IHttpContextAccessor> _httpContextAcessorMock;
        private Fixture _fixture;
        private ServiceUsuario _serviceUsuario;
        private ServiceEmpresa _serviceEmpresa;
        private Mock<IServiceUsuario> _serviceUsuarioMock;
        private Mock<IServiceEmpresa> _serviceEmpresaMock;
        private EmpresaController _empresaController;

        public EmpresaControllerTests()
        {
            _context = ContextMock.CreateApplicationDbContext();
            _httpContextAcessorMock = ContextMock.CreateHttpContextAcessor();
            _fixture = new Fixture();

            _serviceUsuario = new ServiceUsuario(new RepositoryUsuario(_context));
            _serviceEmpresa = new ServiceEmpresa(new RepositoryEmpresa(_context));

            _serviceUsuarioMock = new Mock<IServiceUsuario>();
            _serviceEmpresaMock = new Mock<IServiceEmpresa>();

            _empresaController = new EmpresaController
            (
                _httpContextAcessorMock.Object, 
                _serviceUsuarioMock.Object, 
                _serviceEmpresaMock.Object
            );
        }
                
    }
}

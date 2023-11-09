using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Moq;
using BaseProject.DAO.Data;
using BaseProject.DAO.Models;
using BaseProject.Util;
using System.Security.Claims;

namespace BaseProject.Test.Mocks
{
    public static class ContextMock 
    {
        private static string _IdAspNetUser = Guid.NewGuid().ToString();

        public static ApplicationDbContext CreateApplicationDbContext() 
        {
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            context.Empresa.Add(new Empresa
            {
                Dominio = "jptech.com.br",
                Ativa = true,
                DataCadastro = DateTime.Now.ToBrasiliaTime(),
            });

            context.Usuario.Add(new Usuario
            {
                IdAspNetUser = _IdAspNetUser,
                IdEmpresa = 1,
                Nome = "Admin",
                Email = "admin@jptech.com.br",
                Senha = Guid.NewGuid().ToString(),
                DataCadastro = DateTime.Now.ToBrasiliaTime(),
                Ativo = true,
            });

            context.SaveChanges();

            return context;
        }

        public static Mock<IHttpContextAccessor> CreateHttpContextAcessor()
        {
            var httpContextMock = new Mock<IHttpContextAccessor>();

            httpContextMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim("IdAspNetUser", _IdAspNetUser),
                    new Claim("IdUsuario", "1"),
                    new Claim("IdEmpresa", "1"),
                }, "mock"))
            });

            return httpContextMock;
        }

        public static ControllerContext CreateControllerContext()
        {
            var controllerContext = new Mock<ControllerContext>().Object;

            controllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim("IdAspNetUser", _IdAspNetUser),
                    new Claim("IdUsuario", "1"),
                    new Claim("IdEmpresa", "1"),
                }, "mock"))
            };

            controllerContext.HttpContext.Request.Headers.Referer = new StringValues("test:test");

            return controllerContext;
        }

        public static void CreateControllerContextMock(this Controller controller)
        {
            var controllerContext = new Mock<ControllerContext>().Object;

            controllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim("IdAspNetUser", _IdAspNetUser),
                    new Claim("IdUsuario", "1"),
                    new Claim("IdEmpresa", "1"),
                }, "mock"))
            };

            controllerContext.HttpContext.Request.Headers.Referer = new StringValues("test:test");

            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("https://testurl.com.br").Verifiable();

            controller.ControllerContext = controllerContext;
            controller.Url = urlHelperMock.Object;
        }
    }
}

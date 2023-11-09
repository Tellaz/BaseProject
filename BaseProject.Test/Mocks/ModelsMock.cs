using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BaseProject.DAO.Models;

namespace BaseProject.Test.Mocks
{
    public static class ModelsMock 
    {
        private static readonly Fixture _fixture = new Fixture();

        public static AspNetUser CreateAspNetUser() 
        { 
            var aspNetUser = _fixture.Build<AspNetUser>()
                .Without(x => x.Usuario)
                .Create();

            aspNetUser.Usuario = CreateUsuario();

            return aspNetUser;
        }

        public static AspNetUser[] CreateManyAspNetUser(int numberOfEntitiesToCreate = 10)
        {
            return _fixture.Build<AspNetUser>()
                .Without(x => x.Usuario)
                .CreateMany(numberOfEntitiesToCreate).ToArray();
        }

        public static Empresa CreateEmpresa()
        {
            var empresa = _fixture.Build<Empresa>()
                .Without(x => x.IdRepresentanteNavigation)
                .Without(x => x.EmpresaLogo)
                .Without(x => x.Download)
                .Without(x => x.Upload)
                .Without(x => x.UsuarioIdEmpresaNavigation)
                .Without(x => x.UsuarioIdEmpresaSelecionadaNavigation)
                .Create();

            return empresa;
        }

        public static Empresa[] CreateManyEmpresa(int numberOfEntitiesToCreate = 10)
        {
            return _fixture.Build<Empresa>()
                .Without(x => x.IdRepresentanteNavigation)
                .Without(x => x.EmpresaLogo)
                .Without(x => x.Download)
                .Without(x => x.Upload)
                .Without(x => x.UsuarioIdEmpresaNavigation)
                .Without(x => x.UsuarioIdEmpresaSelecionadaNavigation)
                .CreateMany(numberOfEntitiesToCreate).ToArray();
        }

        public static Usuario CreateUsuario()
        {
            var usuario = _fixture.Build<Usuario>()
                .Without(x => x.Download)
                .Without(x => x.Empresa)
                .Without(x => x.IdAspNetUserNavigation)
                .Without(x => x.IdEmpresaNavigation)
                .Without(x => x.IdEmpresaSelecionadaNavigation)
                .Without(x => x.LogAcessoUsuario)
                .Without(x => x.LogOpenAI)
                .Without(x => x.Upload)
                .Without(x => x.UsuarioFoto)
                .Create();

            usuario.IdEmpresaNavigation = CreateEmpresa();

            return usuario;
        }

        public static Usuario[] CreateManyUsuario(int numberOfEntitiesToCreate = 10)
        {
            return _fixture.Build<Usuario>()
                .Without(x => x.Download)
                .Without(x => x.Empresa)
                .Without(x => x.IdAspNetUserNavigation)
                .Without(x => x.IdEmpresaNavigation)
                .Without(x => x.IdEmpresaSelecionadaNavigation)
                .Without(x => x.LogAcessoUsuario)
                .Without(x => x.LogOpenAI)
                .Without(x => x.Upload)
                .Without(x => x.UsuarioFoto)
                .CreateMany(numberOfEntitiesToCreate).ToArray();
        }

    }
}

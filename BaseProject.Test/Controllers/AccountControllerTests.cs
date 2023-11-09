using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using BaseProject.API.Controllers;
using BaseProject.DAO.Data;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models.Views.Account;
using BaseProject.DAO.Repository;
using BaseProject.DAO.Service;
using BaseProject.Test.Mocks;
using BaseProject.Util;
using Wangkanai.Detection.Services;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BaseProject.Test.Controllers
{
    public class AccountControllerTests
    {
        private ApplicationDbContext _context;
        private Fixture _fixture;
        private IMemoryCache _memoryCache;
        private Mock<UserManagerMock> _userManagerMock;
        private Mock<SignInManagerMock> _signInManagerMock;
        private ServiceUsuario _serviceUsuario;
        private ServiceEmpresa _serviceEmpresa;
        private ServiceLogAcessoUsuario _serviceLogAcessoUsuario;
        private ServiceUpload _serviceUpload;
        private ServiceDownload _serviceDownload;     
        private ServiceAspNetUser _serviceAspNetUser;        
        private Mock<IServiceEmail> _serviceEmailMock;
        private Mock<IServiceToken> _serviceTokenMock;
        private Mock<IDetectionService> _serviceDetectionMock;        
        private Mock<IServiceUsuario> _serviceUsuarioMock;
        private Mock<IServiceEmpresa> _serviceEmpresaMock;
        private Mock<IServiceLogAcessoUsuario> _serviceLogAcessoUsuarioMock;
        private Mock<IServiceUpload> _serviceUploadMock;
        private Mock<IServiceDownload> _serviceDownloadMock;
        private Mock<IServiceAspNetUser> _serviceAspNetUserMock;
        private AccountController _accountController;

        public AccountControllerTests()
        {
            _context = ContextMock.CreateApplicationDbContext();
            _fixture = new Fixture();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _userManagerMock = new Mock<UserManagerMock>();
            _signInManagerMock = new Mock<SignInManagerMock>();

            _serviceEmailMock = new Mock<IServiceEmail>();
            _serviceTokenMock = new Mock<IServiceToken>();
            _serviceDetectionMock = ServicesMock.CreateDetectionService();
            
            _serviceUsuario = new ServiceUsuario(new RepositoryUsuario(_context));
            _serviceEmpresa = new ServiceEmpresa(new RepositoryEmpresa(_context));
            _serviceLogAcessoUsuario = new ServiceLogAcessoUsuario(new RepositoryLogAcessoUsuario(_context));
            _serviceUpload = new ServiceUpload(new RepositoryUpload(_context));
            _serviceDownload = new ServiceDownload(new RepositoryDownload(_context));
            _serviceAspNetUser = new ServiceAspNetUser(new RepositoryAspNetUser(_context));

            _serviceUsuarioMock = new Mock<IServiceUsuario>();
            _serviceEmpresaMock = new Mock<IServiceEmpresa>();
            _serviceLogAcessoUsuarioMock = new Mock<IServiceLogAcessoUsuario>();
            _serviceUploadMock = new Mock<IServiceUpload>();
            _serviceDownloadMock = new Mock<IServiceDownload>();
            _serviceAspNetUserMock = new Mock<IServiceAspNetUser>();

            _accountController = new AccountController
            (
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _serviceEmailMock.Object,
                _serviceUsuarioMock.Object,
                _serviceEmpresaMock.Object,
                _serviceTokenMock.Object,
                _serviceDetectionMock.Object,
                _serviceLogAcessoUsuarioMock.Object,
                _memoryCache,
                _serviceUploadMock.Object,
                _serviceDownloadMock.Object,
                _serviceAspNetUserMock.Object
            );
            _accountController.CreateControllerContextMock();
        }

        [Fact]
        public async Task SignUp_ErroDominioInvalido()
        {
            var password = StringExtensions.RandomPassword(8);

            var model = new SignUpVM
            {
                Nome = Guid.NewGuid().ToString(),
                Sobrenome = Guid.NewGuid().ToString(),
                Telefone = "00000000000",
                Email = "",
                Password = password,
                ConfirmPassword = password,
            };

            var actionResult = await _accountController.SignUp(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Domínio de email inválido!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignUp_ErroEmailJaExiste()
        {
            _serviceUsuarioMock.Setup(x => x.ExisteEmail(It.IsAny<string>())).Returns(true);

            var password = StringExtensions.RandomPassword(8);

            var model = new SignUpVM
            {
                Nome = Guid.NewGuid().ToString(),
                Sobrenome = Guid.NewGuid().ToString(),
                Telefone = "00000000000",
                Email = "test@test.com.br",
                Password = password,
                ConfirmPassword = password,
            };

            var actionResult = await _accountController.SignUp(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Esse email já está vinculado a uma conta!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignUp_SucessoCriarConta()
        {
            var password = StringExtensions.RandomPassword(8);

            var model = new SignUpVM
            {
                Nome = Guid.NewGuid().ToString(),
                Sobrenome = Guid.NewGuid().ToString(),
                Telefone = "00000000000",
                Email = "test@test.com.br",
                Password = password,
                ConfirmPassword = password,
            };

            _serviceEmpresaMock.Setup(x => x.Adicionar(It.IsAny<Empresa>())).Returns(_serviceEmpresa.Adicionar);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));
            _userManagerMock.Setup(x => x.AddToRolesAsync(It.IsAny<AspNetUser>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(IdentityResult.Success));

            var actionResult = await _accountController.SignUp(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Sucesso ao criar a conta!", responseObject.Message);
        }

        [Fact]
        public async Task SignIn_ErroUsuarioNaoEncontrado()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Email ou senha incorreto!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignIn_ErroContaBloqueada()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();

            IList<string> emptyList = new List<string>();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(true));

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Contains("Devido a várias tentativas de acesso, a sua conta foi bloqueada.", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignIn_ErroEmailOuSenhaIncorreto()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();

            IList<string> emptyList = new List<string>();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Failed));

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Email ou senha incorreto!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignIn_ErroContaDeUsuarioDesativada()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();
            user.Usuario.Ativo = false;            

            IList<string> emptyList = new List<string>();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Sua conta de usuário foi desativada!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignIn_ErroEmpresaDesativada()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();
            user.Usuario.Ativo = true;
            user.Usuario.IdEmpresaNavigation.Ativa = false;

            IList<string> emptyList = new List<string>();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Sua empresa foi desativada! Por favor contate o representante da sua empresa.", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task SignIn_SucessoSemAutenticacaoDeDoisFatores()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();
            user.Usuario.Ativo = true;
            user.Usuario.IdEmpresaNavigation.Ativa = true;

            IList<string> emptyList = new List<string>() { "Administrador" };

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));
            _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _serviceTokenMock.Setup(x => x.GenerateToken(It.IsAny<AspNetUser>(), It.IsAny<List<string>>())).Returns(Guid.NewGuid().ToString());

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<AuthVM>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.NotNull(responseObject.Payload);
        }

        [Fact]
        public async Task SignIn_SucessoComAutenticacaoDeDoisFatores()
        {
            var model = new SignInVM
            {
                Email = "test@test.com.br",
                Password = StringExtensions.RandomPassword(8),
            };

            var user = ModelsMock.CreateAspNetUser();
            user.Usuario.Ativo = true;
            user.Usuario.IdEmpresaNavigation.Ativa = true;

            IList<string> emptyList = new List<string>() { "Administrador" };

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(SignInResult.Success));
            _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(true));
            _serviceEmailMock.Setup(x => x.SendEmail(It.IsAny<EmailOptions>())).Returns(Task.FromResult(true));          

            var actionResult = await _accountController.SignIn(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("two-steps", responseObject.Payload);
        }

        [Fact]
        public async Task TwoSteps_ErroUsuarioNaoEncontrado()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Código expirado!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task TwoSteps_ErroContaBloqueada()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(true));

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Contains("Devido a várias tentativas de acesso, a sua conta foi bloqueada.", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task TwoSteps_ErroCodigoExpirado()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Código expirado! Reenvie um novo código de segurança para tentar novamente!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task TwoSteps_ErroCodigoIncorreto()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));

            _memoryCache.Set(
                user.UserName.ToLower(),
                new TwoStepsVM
                {
                    UserName = Guid.NewGuid().ToString(),
                    Code = Guid.NewGuid().ToString(),
                },
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                    SlidingExpiration = TimeSpan.FromMinutes(3)
                }
            );

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Código de segurança incorreto!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task TwoSteps_ErroAdicionarRepresentante()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();
            user.Usuario.IdEmpresaNavigation.IdRepresentante = null;

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            _serviceEmpresaMock.Setup(x => x.Editar(It.IsAny<Empresa>())).Returns(false);

            _memoryCache.Set( user.UserName.ToLower(), model, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                SlidingExpiration = TimeSpan.FromMinutes(3)
            });

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Desculpe, ocorreu um erro! Por favor, tente novamente ou caso o erro persista, contate nosso suporte.", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task TwoSteps_SucessoCodigoValido()
        {
            var model = new TwoStepsVM
            {
                UserName = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(false));
            IList<string> emptyList = new List<string>();
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(emptyList));
            _serviceTokenMock.Setup(x => x.GenerateToken(It.IsAny<AspNetUser>(), It.IsAny<List<string>>())).Returns(Guid.NewGuid().ToString());

            _memoryCache.Set(user.UserName.ToLower(), model, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                SlidingExpiration = TimeSpan.FromMinutes(3)
            });

            var actionResult = await _accountController.TwoSteps(model);

            var responseObject = ControllerExtensions.GetResponseObject<AuthVM>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.NotNull(responseObject.Payload);
        }

        [Fact]
        public async Task ForgotPassword_SucessoUsuarioNaoEncontrado()
        {
            var model = new ForgotPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Redirect = Guid.NewGuid().ToString(),
            };

            var actionResult = await _accountController.ForgotPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Se você possui uma conta cadastrada nós enviamos um email com as instruções para redefinir sua senha.<br />Por favor, verifique sua caixa de spam!", responseObject.Message);
        }

        [Fact]
        public async Task ForgotPassword_ErroEnviarEmail()
        {
            var model = new ForgotPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Redirect = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(Guid.NewGuid().ToString()));
            _serviceEmailMock.Setup(x => x.SendEmail(It.IsAny<EmailOptions>())).Returns(Task.FromResult(false));

            var actionResult = await _accountController.ForgotPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao enviar o email para redefinir sua senha!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ForgotPassword_SucessoUsuarioEncontrado()
        {
            var model = new ForgotPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Redirect = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<AspNetUser>())).Returns(Task.FromResult(Guid.NewGuid().ToString()));
            _serviceEmailMock.Setup(x => x.SendEmail(It.IsAny<EmailOptions>())).Returns(Task.FromResult(true));

            var actionResult = await _accountController.ForgotPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Se você possui uma conta cadastrada nós enviamos um email com as instruções para redefinir sua senha.<br />Por favor, verifique sua caixa de spam!", responseObject.Message);
        }

        [Fact]
        public async Task ResetPassword_ErroUsuarioNaoEncontrado()
        {
            var password = StringExtensions.RandomPassword(8);

            var model = new ResetPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Password = password,
                ConfirmPassword = password,
                Token = Guid.NewGuid().ToString(),
            };

            var actionResult = await _accountController.ResetPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao redefinir sua senha!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_ErroRedefinirSenha()
        {
            var password = StringExtensions.RandomPassword(8);

            var model = new ResetPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Password = password,
                ConfirmPassword = password,
                Token = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Failed()));

            var actionResult = await _accountController.ResetPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao redefinir sua senha!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_SucessoRedefinirSenha()
        {
            var password = StringExtensions.RandomPassword(8);

            var model = new ResetPasswordVM
            {
                Email = Guid.NewGuid().ToString(),
                Password = password,
                ConfirmPassword = password,
                Token = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));
            _serviceUsuarioMock.Setup(x => x.EditarSenha(It.IsAny<int>(), It.IsAny<string>())).Returns(true);
            _userManagerMock.Setup(x => x.RemoveAuthenticationTokenAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));

            var actionResult = await _accountController.ResetPassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Sucesso ao redefinir sua senha!", responseObject.Message);
        }

        [Fact]
        public async Task ResendSecurityCode_ErroUsuarioNaoEncontrado()
        {
            var model = new ResendSecurityCodeVM
            {
                UserName = Guid.NewGuid().ToString(),
            };

            var actionResult = await _accountController.ResendSecurityCode(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao reenviar o código de segurança!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ResendSecurityCode_ErroCodigoJaSocilitado()
        {
            var model = new ResendSecurityCodeVM
            {
                UserName = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);

            _memoryCache.Set
            (
                user.UserName.ToLower(), 
                new TwoStepsVM(), 
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                    SlidingExpiration = TimeSpan.FromMinutes(3)
                }
            );

            var actionResult = await _accountController.ResendSecurityCode(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Já foi solicitado um código de segurança para sua conta nos ultimos 3 minutos, por favor aguarde um momento e tente novamente!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ResendSecurityCode_ErroReenviarCodigo()
        {
            var model = new ResendSecurityCodeVM
            {
                UserName = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _serviceEmailMock.Setup(x => x.SendEmail(It.IsAny<EmailOptions>())).Returns(Task.FromResult(false));

            var actionResult = await _accountController.ResendSecurityCode(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao reenviar o código de segurança!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ResendSecurityCode_SucessoReenviarCodigo()
        {
            var model = new ResendSecurityCodeVM
            {
                UserName = Guid.NewGuid().ToString(),
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _serviceEmailMock.Setup(x => x.SendEmail(It.IsAny<EmailOptions>())).Returns(Task.FromResult(true));

            var actionResult = await _accountController.ResendSecurityCode(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Sucesso ao reenviar o código de segurança!", responseObject.Message);
        }

        [Fact]
        public async Task ChangePassword_ErroSenhaAtualIncorreta()
        {
            var currentPassword = StringExtensions.RandomPassword(8);
            var newPassword = StringExtensions.RandomPassword(8);

            var model = new ChangePasswordVM
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword,
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            var actionResult = await _accountController.ChangePassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Senha atual incorreta!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_ErroEditarSenha()
        {
            var currentPassword = StringExtensions.RandomPassword(8);
            var newPassword = StringExtensions.RandomPassword(8);

            var model = new ChangePasswordVM
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword,
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _userManagerMock.Setup(x => x.ChangePasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Failed()));

            var actionResult = await _accountController.ChangePassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.False(responseObject.IsRequestSuccessful);
            Assert.Equal("Erro ao editar sua senha!", responseObject.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_SucessoEditarSenha()
        {
            var currentPassword = StringExtensions.RandomPassword(8);
            var newPassword = StringExtensions.RandomPassword(8);

            var model = new ChangePasswordVM
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword,
            };

            var user = ModelsMock.CreateAspNetUser();

            _serviceAspNetUserMock.Setup(x => x.ObterPorUserName(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _userManagerMock.Setup(x => x.ChangePasswordAsync(It.IsAny<AspNetUser>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));

            var actionResult = await _accountController.ChangePassword(model);

            var responseObject = ControllerExtensions.GetResponseObject<string>(actionResult);

            Assert.True(responseObject.IsRequestSuccessful);
            Assert.Equal("Sucesso ao editar sua senha!", responseObject.Message);
        }
    }
}

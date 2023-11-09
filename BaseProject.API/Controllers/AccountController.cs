using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Others;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models.Views.Account;
using BaseProject.Util;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text;
using Wangkanai.Detection.Services;
using Microsoft.AspNetCore.Http;

namespace BaseProject.API.Controllers
{
	[Route("[controller]")]
    public class AccountController : Controller
    {
        private UserManager<AspNetUser> _userManager;
        private SignInManager<AspNetUser> _signInManager;
        private readonly IServiceEmail _serviceEmail;
        private readonly IServiceUsuario _serviceUsuario;
        private readonly IServiceEmpresa _serviceEmpresa;
        private readonly IServiceToken _serviceToken;
        private readonly IDetectionService _detectionService;
        private readonly IServiceLogAcessoUsuario _serviceLogAcessoUsuario;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceUpload _serviceUpload;
        private readonly IServiceDownload _serviceDownload;
        private readonly IServiceAspNetUser _serviceAspNetUser;

        public AccountController(
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            IServiceEmail serviceEmail,
            IServiceUsuario serviceUsuario,
            IServiceEmpresa serviceEmpresa,
            IServiceToken serviceToken,
            IDetectionService detectionService,
            IServiceLogAcessoUsuario serviceLogAcessoUsuario,
            IMemoryCache memoryCache,
            IServiceUpload serviceUpload,
            IServiceDownload serviceDownload,
            IServiceAspNetUser serviceAspNetUser
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _serviceEmail = serviceEmail;
            _serviceUsuario = serviceUsuario;
            _serviceEmpresa = serviceEmpresa;
            _serviceToken = serviceToken;
            _detectionService = detectionService;
            _serviceLogAcessoUsuario = serviceLogAcessoUsuario;
            _memoryCache = memoryCache;
            _serviceUpload = serviceUpload;
            _serviceDownload = serviceDownload;
            _serviceAspNetUser = serviceAspNetUser;
        }

        //Cadastrar conta

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpVM model)
        {
            var dominio = model.Email.GetEmailDominio();

            if (string.IsNullOrEmpty(dominio)) return Json(this.CreateResponseObject(false, errorMessage: "Domínio de email inválido!"));

            var existeUsuario = _serviceUsuario.ExisteEmail(model.Email);

            if (existeUsuario) return Json(this.CreateResponseObject(false, errorMessage: "Esse email já está vinculado a uma conta!"));

            bool sucesso = false;

            var empresa = _serviceEmpresa.ObterPorDominio(dominio);

            if (empresa == null)
            {
                empresa = new Empresa
                {
                    DataCadastro = DateTime.Now.ToBrasiliaTime(),
                    Ativa = true,
                    Dominio = dominio,                    
                };

                sucesso = _serviceEmpresa.Adicionar(empresa);

                if (!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Desculpe, houve um erro ao criar sua conta! Por favor, contate nosso suporte."));
            }
            
            var user = new AspNetUser
            {
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                PhoneNumber = model.Telefone,
                TwoFactorEnabled = true,
                Usuario = new Usuario
                {
                    IdEmpresa = empresa.Id,
                    Nome = model.Nome.Trim(),
                    Sobrenome = string.IsNullOrEmpty(model.Sobrenome) ? null : model.Sobrenome.Trim(),
                    Email = model.Email.ToLower(),
                    Senha = Convert.ToBase64String(Encoding.ASCII.GetBytes(model.Password)),
                    DataCadastro = DateTime.Now.ToBrasiliaTime(),
                    Ativo = true,                    
                }
            };

            var chkUser = await _userManager.CreateAsync(user, model.Password);

            sucesso = chkUser.Succeeded;

            return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao criar a conta!", errorMessage: "Desculpe, houve um erro ao criar sua conta! Por favor, contate nosso suporte."));
        }

        //Confirmar email

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var model = new EmailConfirmVM
            {
                Email = email.ToLower()
            };

            if (!string.IsNullOrEmpty(token))
            {
                var user = _serviceAspNetUser.ObterPorEmail(model.Email);

                bool verifyToken = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "EmailConfirmation", token);

                if (!verifyToken) return Json(this.CreateResponseObject<string>(true, "token-expired"));

                token = token.Replace(' ', '+');

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    model.EmailVerified = true;
                }
            }

			return Json(this.CreateResponseObject(true, model));
        }

        private async Task<bool> SendEmailForEmailConfirmation(AspNetUser user)
        {
            try
            {
                await _userManager.UpdateSecurityStampAsync(user);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				var clientUrl = new Uri(Request.Headers.Referer.FirstOrDefault() ?? Request.GetDisplayUrl());

				var emailConfirmLink = Url.Action("confirm-email", "", new { email = user.Email, token = token }, clientUrl.Scheme, clientUrl.Authority);

                emailConfirmLink = emailConfirmLink.Replace("/Dashboard/", "/");

                var emailOptions = new EmailOptions
                {
                    Subject = "Confirmação de email!",
                    ToEmail = user.Email,                    
                    Template = EmailTemplate.ConfirmarEmail,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("{{NOME}}", user.UserName),
                        new KeyValuePair<string, string>("{{URL}}", emailConfirmLink)
                    }
                };

                await _serviceEmail.SendEmail(emailOptions);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmVM model)
        {
			var user = _serviceAspNetUser.ObterPorEmail(model.Email);

            if (user != null)
            {
                if (await _userManager.IsEmailConfirmedAsync(user))
                {
					return Json(this.CreateResponseObject(false, errorMessage: "Esse email já foi confirmado!"));
                }

                bool sucesso = await SendEmailForEmailConfirmation(user);

                if (!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Erro ao enviar o email de confirmação da sua conta!"));
                
            }

			return Json(this.CreateResponseObject(true, successMessage: "Se você possui uma conta cadastrada nós enviamos um email com as instruções para confirma-la.<br />Por favor, verifique sua caixa de spam!"));			
        }

        [Authorize]
        [HttpPost("ValidarPrimeiroAcesso")]
        public IActionResult ValidarPrimeiroAcesso()
        {
            var idUsuario = Int32.Parse(User.FindFirstValue("IdUsuario"));

            var usuario = _serviceUsuario.ObterPorId(idUsuario);
            usuario.PrimeiroAcesso = true;
            var sucesso = _serviceUsuario.Editar(usuario);

            return Json(this.CreateResponseObject(sucesso, errorMessage: "Erro ao validar primeiro acesso!"));
        }

        [Authorize]
        [HttpGet("UpdateAuthModel")]
        public async Task<IActionResult> UpdateAuthModel()
        {
            var idUsuario = Int32.Parse(User.FindFirstValue("IdUsuario"));

            var usuario = _serviceUsuario.ObterPorId(idUsuario);

            var user = _serviceAspNetUser.ObterPorEmail(usuario.Email, "Usuario.IdEmpresaNavigation.EmpresaLogo,Usuario.UsuarioFoto");

            var roles = await _userManager.GetRolesAsync(user);

            int idEmpresa = 0;

            if (roles.Contains("Administrador") && usuario.IdEmpresaSelecionada.HasValue)
            {
                var empresa = _serviceEmpresa.ObterPorId(usuario.IdEmpresaSelecionada.Value, "EmpresaLogo");
                user.Usuario.IdEmpresaNavigation = empresa;
                idEmpresa = usuario.IdEmpresaSelecionada.Value;
            }
            else idEmpresa = usuario.IdEmpresa;

            return Json(this.CreateResponseObject(true, new AuthVM(new UserVM(user), roles.ToArray())));
        }

        //Entrar na conta

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInVM model)
        {
			var user = _serviceAspNetUser.ObterPorEmail(model.Email, "Usuario.IdEmpresaNavigation.EmpresaLogo,Usuario.UsuarioFoto");

            var roles = await _userManager.GetRolesAsync(user);

            if (user == null) return Json(this.CreateResponseObject(false, errorMessage: "Email ou senha incorreto!"));

            if (await _userManager.IsLockedOutAsync(user))
            {
                var now = DateTimeOffset.Now;
                var end = user.LockoutEnd.Value;
                var minutes = Math.Ceiling((end - now).TotalMinutes);

				return Json(this.CreateResponseObject(false, errorMessage: $"Devido a várias tentativas de acesso, a sua conta foi bloqueada. Tente novamente em {minutes + (minutes == 1 ? " minuto" : " minutos")}!"));				
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

            var usuario = user.Usuario;

            if (!result.Succeeded)
			{
                GerarLogAcessoUsuario(usuario.Id, false);

				return Json(this.CreateResponseObject(false, errorMessage: "Email ou senha incorreto!"));
            }

            if (!usuario.Ativo) return Json(this.CreateResponseObject(false, errorMessage: "Sua conta de usuário foi desativada!"));

            if (!usuario.IdEmpresaNavigation.Ativa) return Json(this.CreateResponseObject(false, errorMessage: "Sua empresa foi desativada! Por favor contate o representante da sua empresa."));

            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

			if (!twoFactorEnabled)
			{
                var token = _serviceToken.GenerateToken(user, roles.ToList());

                Response.Cookies.Delete("redirectUrl");
                Response.Cookies.Append("access_token", token);

                GerarLogAcessoUsuario(usuario.Id, true);

                return Json(this.CreateResponseObject(true, new AuthVM(new UserVM(user), roles.ToArray())));
            }

            var existsSecurityCode = _memoryCache.TryGetValue(user.UserName.ToLower(), out TwoStepsVM twoStepsVM);

            if (existsSecurityCode) _memoryCache.Remove(user.UserName.ToLower());

            twoStepsVM = new TwoStepsVM
            {
                UserName = user.UserName.ToLower(),
                Code = StringExtensions.RandomNumber(6)
            };

            _memoryCache.Set(user.UserName.ToLower(), twoStepsVM, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                SlidingExpiration = TimeSpan.FromMinutes(3)
            });

            var sucesso = await SendEmailSecutiryCode(user, twoStepsVM.Code);

			return Json(this.CreateResponseObject<string>(true, "two-steps"));
		}

        //Verificação em duas etapas

        [HttpPost("TwoSteps")]
        public async Task<IActionResult> TwoSteps([FromBody] TwoStepsVM model)
        {
			var user = _serviceAspNetUser.ObterPorUserName(model.UserName, "Usuario.IdEmpresaNavigation.EmpresaLogo,Usuario.UsuarioFoto");

            if (user == null) return Json(this.CreateResponseObject(false, errorMessage: "Código expirado!"));

			if (await _userManager.IsLockedOutAsync(user))
            {
                var now = DateTimeOffset.Now;
                var end = user.LockoutEnd.Value;
                var minutes = Math.Ceiling((end - now).TotalMinutes);

				return Json(this.CreateResponseObject(false, errorMessage: $"Devido a várias tentativas de acesso, a sua conta foi bloqueada. Tente novamente em {minutes + (minutes == 1 ? " minuto" : " minutos")}!"));				
            }

            var existsSecurityCode = _memoryCache.TryGetValue(user.UserName.ToLower(), out TwoStepsVM twoStepsVM);

            if (!existsSecurityCode)
            {
				return Json(this.CreateResponseObject(false, errorMessage: "Código expirado! Reenvie um novo código de segurança para tentar novamente!"));				
            }

            if(model.UserName.ToLower() != twoStepsVM.UserName.ToLower() || model.Code != twoStepsVM.Code)
			{
                await _signInManager.PasswordSignInAsync(user, "", true, true); //Gera tentativa de acesso para bloqueio da conta

				return Json(this.CreateResponseObject(false, errorMessage: "Código de segurança incorreto!"));
            }

            var usuario = user.Usuario;

            var empresa = usuario.IdEmpresaNavigation;

            if(!empresa.IdRepresentante.HasValue)
            {
                empresa.IdRepresentante = usuario.Id;

                bool sucesso = _serviceEmpresa.Editar(empresa);

                if (!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Desculpe, ocorreu um erro! Por favor, tente novamente ou caso o erro persista, contate nosso suporte."));
            }

            _memoryCache.Remove(user.UserName.ToLower());
            
            var roles = await _userManager.GetRolesAsync(user);
            var token = _serviceToken.GenerateToken(user, roles.ToList());

            Response.Cookies.Delete("redirectUrl");
            Response.Cookies.Append("access_token", token);

            GerarLogAcessoUsuario(usuario.Id, true);

            return Json(this.CreateResponseObject(true, new AuthVM(new UserVM(user), roles.ToArray())));
        }

        [HttpPost("ResendSecurityCode")]
        public async Task<IActionResult> ResendSecurityCode([FromBody] ResendSecurityCodeVM model)
        {
			var user = _serviceAspNetUser.ObterPorUserName(model.UserName, "Usuario.IdEmpresaNavigation.EmpresaLogo,Usuario.UsuarioFoto");

            if (user == null) return Json(this.CreateResponseObject(false, errorMessage: "Erro ao reenviar o código de segurança!"));

			var existsSecurityCode = _memoryCache.TryGetValue(user.UserName.ToLower(), out TwoStepsVM twoStepsVM);

            if (existsSecurityCode) return Json(this.CreateResponseObject(false, errorMessage: "Já foi solicitado um código de segurança para sua conta nos ultimos 3 minutos, por favor aguarde um momento e tente novamente!"));

			twoStepsVM = new TwoStepsVM
            {
                UserName = user.UserName.ToLower(),
                Code = StringExtensions.RandomNumber(6)
            };

            _memoryCache.Set(user.UserName.ToLower(), twoStepsVM, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                SlidingExpiration = TimeSpan.FromMinutes(3)
            });

            var sucesso = await SendEmailSecutiryCode(user, twoStepsVM.Code);

            return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao reenviar o código de segurança!", errorMessage: "Erro ao reenviar o código de segurança!"));            
        }

        private async Task<bool> SendEmailSecutiryCode(AspNetUser user, string codigo)
        {
            try
            {
                var emailOptions = new EmailOptions
                {
                    Subject = "Código de segurança!",
                    ToEmail = user.Email,
                    Template = EmailTemplate.CodigoSeguranca,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("{{NOME}}", user.Usuario.Nome),
                        new KeyValuePair<string, string>("{{CODIGO}}", codigo)
                    }
                };

                var sucesso = await _serviceEmail.SendEmail(emailOptions);

                return sucesso;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        //Gerar log de acesso
        private bool GerarLogAcessoUsuario(int idUsuario, bool status)
		{
            var enderecoIP = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            if (string.IsNullOrEmpty(enderecoIP) || enderecoIP == "::1") enderecoIP = "127.0.0.1";

            var logAcessoUsuario = new LogAcessoUsuario
            {
                IdUsuario = idUsuario,
                EnderecoIP = enderecoIP,
                Dispositivo = _detectionService.Device.Type.ToString(),
                Plataforma = _detectionService.Platform.Name + " " + _detectionService.Platform.Version.ToString() + " " + _detectionService.Platform.Processor.ToString(),
                Navegador = _detectionService.Browser.Name + " " + _detectionService.Browser.Version.ToString(),
                Status = status,
                Data = DateTime.Now.ToBrasiliaTime(),
            };

            return _serviceLogAcessoUsuario.Adicionar(logAcessoUsuario);
        }        

        //Redefinir a senha

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVM model)
        {
			var user = _serviceAspNetUser.ObterPorEmail(model.Email, "Usuario");

            if (user != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

				var clientUrl = new Uri(Request.Headers.Referer.FirstOrDefault() ?? Request.GetDisplayUrl());

				var passwordResetLink = Url.Action("reset-password", "", model.Redirect != "/" ? new { email = model.Email, token = token, redirect = model.Redirect } : new { email = model.Email, token = token }, clientUrl.Scheme, clientUrl.Authority);

                passwordResetLink = passwordResetLink.Replace("/Dashboard/", "/");

                var emailOptions = new EmailOptions
                {
                    Subject = "Redefinição de senha!",
                    ToEmail = user.Email,
                    Template = EmailTemplate.RedefinirSenha,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("{{NOME}}", user.Usuario.Nome),
                        new KeyValuePair<string, string>("{{URL}}", passwordResetLink)
                    }
                };

                bool sucesso = await _serviceEmail.SendEmail(emailOptions);

				if (!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Erro ao enviar o email para redefinir sua senha!"));
			}

			return Json(this.CreateResponseObject(true, successMessage: "Se você possui uma conta cadastrada nós enviamos um email com as instruções para redefinir sua senha.<br />Por favor, verifique sua caixa de spam!"));
			
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM model)
        {
			var user = _serviceAspNetUser.ObterPorEmail(model.Email, "Usuario");

            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    var usuario = user.Usuario;

                    _serviceUsuario.EditarSenha(usuario.Id, model.Password);

                    await _userManager.RemoveAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword");

                    return Json(this.CreateResponseObject(true, successMessage: "Sucesso ao redefinir sua senha!"));
                }
            }

			return Json(this.CreateResponseObject(false, errorMessage: "Erro ao redefinir sua senha!"));
        }

        //Mudar senha

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
			var user = _serviceAspNetUser.ObterPorUserName(User.Identity.Name, "Usuario");

            bool sucesso = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

			if (!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Senha atual incorreta!"));

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);            

            sucesso = result.Succeeded;

            if (sucesso)
            {
                var usuario = user.Usuario;

                _serviceUsuario.EditarSenha(usuario.Id, model.NewPassword);
            }

			return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao editar sua senha!", errorMessage: "Erro ao editar sua senha!"));
        }

        [Authorize]
        [HttpPost("ListarLogAcessoUsuario")]
        public IActionResult ListarLogAcessoUsuario([FromBody] DTParam param)
        {
            var idUsuario = Int32.Parse(User.FindFirstValue("IdUsuario"));

            var result = _serviceLogAcessoUsuario.Listar(param, idUsuario);

            return Json(this.CreateResponseObject(true, new
            {
                total = result.Total,
                data = result.Itens
            }));
        }

        [Authorize]
        [HttpGet("GetAvatar")]
        public IActionResult GetAvatar()
        {
			return Json(this.CreateResponseObject(true, _serviceAspNetUser.ObterPorUserName(User.Identity.Name, "Usuario.UsuarioFoto").Usuario.UsuarioFoto));
        }

        [Authorize]
        [HttpPost("ChangeAvatar")]
        public IActionResult ChangeAvatar([FromBody] UsuarioFoto model)
        {
            var usuario = _serviceAspNetUser.ObterPorUserName(User.Identity.Name, "Usuario.UsuarioFoto").Usuario;

            usuario.UsuarioFoto = model;

            bool sucesso = _serviceUsuario.Editar(usuario);

            var fotoUrl = "data:" + usuario.UsuarioFoto.Tipo + ";base64," + usuario.UsuarioFoto.Base64;

            return Json(this.CreateResponseObject(sucesso, fotoUrl, successMessage: "Sucesso ao editar sua foto!", errorMessage: "Erro ao editar sua foto!"));
        }

        [Authorize]
        [HttpPost("ExcluirAvatar")]
        public IActionResult ExcluirAvatar()
        {
            var usuario = _serviceAspNetUser.ObterPorUserName(User.Identity.Name, "Usuario.UsuarioFoto").Usuario;

            usuario.UsuarioFoto = null;

            bool sucesso = _serviceUsuario.Editar(usuario);

            return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao exluir sua foto!", errorMessage: "Erro ao exluir sua foto!"));
        }

        [Authorize]
        [HttpPost("AlterarDoisFatores")]
        public async Task<IActionResult> AlterarDoisFatores()
        {
            var user = _serviceAspNetUser.ObterPorUserName(User.Identity.Name);

            var enabled = !user.TwoFactorEnabled;

            var result = await _userManager.SetTwoFactorEnabledAsync(user, enabled);

            return Json(this.CreateResponseObject(result.Succeeded, successMessage: $"Sucesso ao {(enabled ? "habilitar" : "desabilitar")} a autenticação de dois fatores!", errorMessage: $"Erro ao {(enabled ? "habilitar" : "desabilitar")} a autenticação de dois fatores!"));
        }

        [Authorize]
        [HttpPost("ListarUploads")]
        public IActionResult ListarUploads([FromBody] DTParam<UploadFM> param)
        {
            var idEmpresa = _serviceUsuario.ObterIdEmpresaSelecionada(HttpContext);
            
            var result = _serviceUpload.Listar(param, idEmpresa);

            return Json(this.CreateResponseObject(true, new
            {
                total = result.Total,
                data = result.Itens
            }));
        }

        [Authorize]
        [HttpGet("BaixarUploadArquivo")]
        public IActionResult BaixarUploadArquivo([FromQuery] int id)
        {
            var upload = _serviceUpload.ObterPorId(id, "UploadArquivo");

            if (upload.Status == (byte)EnumTaskStatus.Processando) return Json(this.CreateResponseObject(false, errorMessage: "O arquivo ainda está sendo processado!"));

            var arquivo = upload.UploadArquivo;

            return Json(this.CreateResponseObject(arquivo != null, arquivo, errorMessage: "Desculpe, houve um erro ao baixar o arquivo! Por favor contate nosso suporte."));
        }

        [Authorize]
        [HttpPost("ListarDownloads")]
        public IActionResult ListarDownloads([FromBody] DTParam<DownloadFM> param)
        {
            var idEmpresa = _serviceUsuario.ObterIdEmpresaSelecionada(HttpContext);

            var result = _serviceDownload.Listar(param, idEmpresa);

            return Json(this.CreateResponseObject(true, new
            {
                total = result.Total,
                data = result.Itens
            }));
        }

        [Authorize]
        [HttpGet("BaixarDownloadArquivo")]
        public IActionResult BaixarDownloadArquivo([FromQuery] int id)
        {
            var download = _serviceDownload.ObterPorId(id, "DownloadArquivo");

            if (download.Status == (byte)EnumTaskStatus.Processando) return Json(this.CreateResponseObject(false, errorMessage: "O arquivo ainda está sendo processado!"));

            var arquivo = download.DownloadArquivo;

            return Json(this.CreateResponseObject(arquivo != null, arquivo, errorMessage: "Desculpe, houve um erro ao baixar o arquivo! Por favor contate nosso suporte."));
        }

    }
}

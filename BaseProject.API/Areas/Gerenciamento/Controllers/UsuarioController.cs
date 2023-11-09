using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using Microsoft.AspNetCore.Authorization;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models.Others;
using BaseProject.Util;
using System.Text;
using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http.Extensions;

namespace BaseProject.API.Areas.Gerenciamento.Controllers
{
    [Authorize]
    [Area("Gerenciamento")]
    [Route("[area]/[controller]")]
    public class UsuarioController : Controller
    {
        private readonly int _idUsuario;
        private readonly int _idEmpresa;
        private UserManager<AspNetUser> _userManager;
        private readonly IServiceAspNetUser _serviceAspNetUser;
        private readonly IServiceUsuario _serviceUsuario;
        private readonly IServiceEmpresa _serviceEmpresa;
        private readonly IServiceEmail _serviceEmail;
        private readonly IServiceUpload _serviceUpload;
        private readonly IServiceBusSender _serviceBusSender;

        public UsuarioController(
            IHttpContextAccessor httpContextAccessor,
            UserManager<AspNetUser> userManager,
            IServiceAspNetUser serviceAspNetUser,
            IServiceUsuario serviceUsuario,
            IServiceEmpresa serviceEmpresa,
            IServiceEmail serviceEmail,
            IServiceUpload serviceUpload,
            IServiceBusSender serviceBusSender
        )
        {
            _idUsuario = Int32.Parse(httpContextAccessor.HttpContext.User.FindFirstValue("IdUsuario"));
            _idEmpresa = serviceUsuario.ObterIdEmpresaSelecionada(httpContextAccessor.HttpContext);
            _userManager = userManager;
            _serviceAspNetUser = serviceAspNetUser;
            _serviceUsuario = serviceUsuario;
            _serviceEmpresa = serviceEmpresa;
            _serviceEmail = serviceEmail;
            _serviceUpload = serviceUpload;
            _serviceBusSender = serviceBusSender;
        }

        [HttpPost("Listar")]
        public IActionResult Listar([FromBody] DTParam<UsuarioFM> param)
        {
            var result = _serviceUsuario.Listar(param, _idEmpresa);

            return Json(this.CreateResponseObject(true, new
            {
                total = result.Total,
                data = result.Itens
            }));
        }

        [HttpGet("BaixarModeloCarregar")]
        public IActionResult BaixarModeloCarregar()
        {
            var totalColunas = 9;

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("USUÁRIOS");

            worksheet.Row(1).Height = 25;
            worksheet.SheetView.FreezeRows(1);

            worksheet.Columns(1, totalColunas).Width = 80;

            worksheet.Columns(1, totalColunas).Style
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText(true);

            worksheet.Cell(1, 1).SetValue("NOME *").Style.Font.SetFontSize(14).Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromArgb(1, 35, 40, 71));
            worksheet.Cell(1, 2).SetValue("SOBRENOME *").Style.Font.SetFontSize(14).Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromArgb(1, 35, 40, 71));
            worksheet.Cell(1, 3).SetValue("EMAIL *").Style.Font.SetFontSize(14).Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromArgb(1, 35, 40, 71));
            worksheet.Cell(1, 4).SetValue("CPF *").Style.Font.SetFontSize(14).Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromArgb(1, 35, 40, 71));
            
            worksheet.Range(worksheet.Cell(1, 1), worksheet.Cell(1, totalColunas)).Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

            using var ms = new MemoryStream();

            workbook.SaveAs(ms);
            workbook.Dispose();

            byte[] bytes = ms.ToArray();

            return Json(this.CreateResponseObject(true, new ArquivoVM
            {
                Nome = "MODELO_CARREGAR_USUÁRIOS.xlsx",
                Extensao = "xlsx",
                Tamanho = bytes.Length,
                Tipo = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Base64 = Convert.ToBase64String(bytes),
            }));
        }

        [HttpPost("Carregar")]
        public async Task<IActionResult> Carregar([FromBody] CarregarUsuariosVM model)
        {
            var processando = _serviceUpload.Processando(_idEmpresa);

            if (processando) return Json(this.CreateResponseObject(false, errorMessage: "Desculpe, existe um processo de upload em andamento! Por favor aguarde o processo finalizar e tente novamente."));

            var arquivo = model.Arquivo;

            try
            {
                var totalColunas = 9;

                if (arquivo.Extensao != "xlsx" || arquivo.Tipo != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") return Json(this.CreateResponseObject(false, errorMessage: "Tipo de arquivo inválido!"));

                using var ms = new MemoryStream(Convert.FromBase64String(arquivo.Base64));

                using var workbook = new XLWorkbook(ms);

                var worksheet = workbook.Worksheet("USUÁRIOS");

                var ultimaLinha = worksheet.LastRowUsed().RowNumber();

                if (ultimaLinha <= 1) return Json(this.CreateResponseObject(false, errorMessage: "Linhas do modelo inválidas ou vazias!"));

                if (ultimaLinha > 501) return Json(this.CreateResponseObject(false, errorMessage: "Só é possível carregar no máximo 500 usuários por planilha!"));

                var ultimaColuna = worksheet.LastColumnUsed().ColumnNumber();

                if (ultimaColuna != totalColunas ||
                    worksheet.Cell(1, 1).GetString().RemoveSpace() != "NOME *" ||
                    worksheet.Cell(1, 2).GetString().RemoveSpace() != "SOBRENOME *" ||
                    worksheet.Cell(1, 3).GetString().RemoveSpace() != "EMAIL *" ||
                    worksheet.Cell(1, 4).GetString().RemoveSpace() != "CPF *" )
                    return Json(this.CreateResponseObject(false, errorMessage: "Colunas do modelo inválidas!"));
            }
            catch
            {
                return Json(this.CreateResponseObject(false, errorMessage: "Erro ao verificar o modelo! Tenha certeza que está utilizando o modelo baixado e preencheendo os dados corretamente sem alterar o nome das colunas ou da planilha. Caso o erro persista, contate nosso suporte."));
            }

            var upload = new Upload
            {
                IdUsuario = _idUsuario,
                IdEmpresa = _idEmpresa,
                MD5 = Cryptography.CreateMD5(arquivo.Base64),
                Tipo = (byte)EnumUploadTipo.Usuario,
                Status = (byte)EnumTaskStatus.Processando,
                DataInicial = DateTime.Now.ToBrasiliaTime(),
            };

            var sucesso = _serviceUpload.Adicionar(upload);

            if (sucesso)
            {
                var message = new ServiceBusMessageUpload<CarregarUsuariosVM>(upload.Id, model);

                sucesso = await _serviceBusSender.SendMessage((byte)EnumAzureServiceBusQueue.CarregarUsuarios, message);

                if (!sucesso) _serviceUpload.Deletar(upload.Id);
            }

            return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao iniciar o processo de upload!", errorMessage: "Erro ao iniciar o processo de upload! Por favor tente novamente ou contate nosso suporte."));
        }

        [HttpPost("Adicionar")]
        public async Task<IActionResult> Adicionar([FromBody] UsuarioVM model)
        {
			var processando = _serviceUpload.Processando(_idEmpresa);

			if (processando) return Json(this.CreateResponseObject(false, errorMessage: "Desculpe, existe um processo de upload em andamento! Por favor aguarde o processo finalizar e tente novamente."));

			var empresa = _serviceEmpresa.ObterPorId(_idEmpresa);

            var emailDominio = model.Email.GetEmailDominio() ?? "";

			if (empresa.Dominio.ToLower() != emailDominio.ToLower()) return Json(this.CreateResponseObject(false, errorMessage: "O domínio do email não pertence a empresa!"));
            
            var existeEmail = _serviceUsuario.ExisteEmail(model.Email);

            if (existeEmail) return Json(this.CreateResponseObject(false, errorMessage: "Esse email já está vinculado a uma conta!"));

            var existeCPF = _serviceUsuario.ExisteCPF(model.CPF);

            if (existeCPF) return Json(this.CreateResponseObject(false, errorMessage: "Esse CPF já está vinculado a uma conta!"));

            var senha = StringExtensions.RandomPassword(14);

            var user = new AspNetUser
            {
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                TwoFactorEnabled = true,
                Usuario = new Usuario
                {   
                    IdEmpresa = _idEmpresa,
					Nome = model.Nome.Trim(),
					Sobrenome = string.IsNullOrEmpty(model.Sobrenome) ? null : model.Sobrenome.Trim(),
					Email = model.Email.ToLower(),
                    CPF = model.CPF,
                    Senha = Convert.ToBase64String(Encoding.ASCII.GetBytes(senha)),
                    DataCadastro = DateTime.Now.ToBrasiliaTime(),
                    Ativo = model.Ativo,
                    UsuarioFoto = model.UsuarioFoto,
                }
            };

            var chkUser = await _userManager.CreateAsync(user, senha);

            bool sucesso = chkUser.Succeeded;

            if (sucesso)
            {
				var clientUrl = new Uri(Request.Headers.Referer.FirstOrDefault() ?? Request.GetDisplayUrl());

				var emailOptions = new EmailOptions
                {
                    Subject = "Dados de acesso a sua conta!",
                    ToEmail = user.Email,
                    Template = EmailTemplate.NovoUsuario,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("{{NOME}}", user.Usuario.Nome),
                        new KeyValuePair<string, string>("{{LOGIN}}", user.UserName),
                        new KeyValuePair<string, string>("{{SENHA}}", senha),
                        new KeyValuePair<string, string>("{{URL}}", clientUrl.AbsoluteUri)
                    }
                };

                sucesso = await _serviceEmail.SendEmail(emailOptions);

                if(!sucesso) return Json(this.CreateResponseObject(false, errorMessage: "Sucesso ao adicionar o usuário, mas houve um erro ao enviar o email com os dados de acesso a sua conta. Por favor, contate um administrador!"));

            }

            return Json(this.CreateResponseObject(sucesso, successMessage: $"Sucesso ao adicionar o usuário!", errorMessage: $"Erro ao adicionar o usuário!"));
        }

		[HttpPut("Editar")]
		public async Task<IActionResult> Editar([FromBody] UsuarioVM model)
		{
            var processando = _serviceUpload.Processando(_idEmpresa);

			if (processando) return Json(this.CreateResponseObject(false, errorMessage: "Desculpe, existe um processo de upload em andamento! Por favor aguarde o processo finalizar e tente novamente."));

			var user = _serviceAspNetUser.ObterPorId(model.Id, "Usuario.UsuarioPerfil,Usuario.UsuarioFoto");

			if (user.Usuario.Email.ToLower() != model.Email.ToLower())
			{
                var empresa = _serviceEmpresa.ObterPorId(_idEmpresa);

                var emailDominio = model.Email.GetEmailDominio() ?? "";

                if (empresa.Dominio.ToLower() != emailDominio.ToLower()) return Json(this.CreateResponseObject(false, errorMessage: "O domínio do email não pertence a empresa!"));

                var existeEmail = _serviceUsuario.ExisteEmail(model.Email);

				if (existeEmail) return Json(this.CreateResponseObject(false, errorMessage: "Esse email já está vinculado a uma conta!"));
			}

			if (user.Usuario.CPF == null || user.Usuario.CPF.ToLower() != model.CPF.ToLower())
			{
				var existeCPF = _serviceUsuario.ExisteCPF(model.CPF);

				if (existeCPF) return Json(this.CreateResponseObject(false, errorMessage: "Esse CPF já está vinculado a uma conta!"));
			}

            user.UserName = model.Email.ToLower();
			user.Email = model.Email.ToLower();
			user.Usuario.IdEmpresa = _idEmpresa;
			user.Usuario.Nome = model.Nome;
			user.Usuario.Sobrenome = model.Sobrenome;
			user.Usuario.Email = model.Email.ToLower();
			user.Usuario.CPF = model.CPF;
            user.Usuario.UsuarioFoto = model.UsuarioFoto;

			var chkUser = await _userManager.UpdateAsync(user);

			bool sucesso = chkUser.Succeeded;

			return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao editar o usuário!", errorMessage: "Erro ao editar o usuário!"));
		}

		[HttpDelete("Deletar")]
        public async Task<IActionResult> Deletar([FromBody] int id)
        {
            var user = _serviceAspNetUser.ObterPorId(id, "Usuario");

            var chkUser = await _userManager.DeleteAsync(user);

            bool sucesso = chkUser.Succeeded;

			return Json(this.CreateResponseObject(sucesso, successMessage: $"Sucesso ao deletar o usuário!", errorMessage: $"Erro ao deletar o usuário!"));			
        }

        [HttpGet("ObterFoto")]
        public IActionResult ObterFoto([FromQuery] int idUsuario)
        {
            UsuarioFoto[] result = { _serviceUsuario.ObterPorId(idUsuario, "UsuarioFoto").UsuarioFoto };

            return Json(this.CreateResponseObject(true, result));
        }

		[HttpGet("ObterParaSelect")]
		public IActionResult ObterParaSelect()
		{
			return Json(this.CreateResponseObject(true, _serviceUsuario.ObterParaSelect(_idEmpresa)));
		}

		[HttpGet("ObterPorId")]
		public IActionResult ObterPorId([FromQuery] int idUsuario)
		{
			var usuario = _serviceUsuario.ObterPorId(idUsuario, "UsuarioPerfil,UsuarioPerfil.IdPerfilNavigation,UsuarioFoto");

			var dados = new UsuarioVM(usuario);

			return Json(this.CreateResponseObject(true, payload: dados));
		}

		[HttpPut("Desabilitar")]
		public IActionResult Desabilitar([FromQuery] int idUsuario)
		{
			var usuario = _serviceUsuario.ObterPorId(idUsuario);

            usuario.Ativo = !usuario.Ativo;

			bool success = _serviceUsuario.Editar(usuario);

			return Json(this.CreateResponseObject(success, successMessage: $"Usuário {(usuario.Ativo ? "habilitado" : "desabilitado")}  com sucesso!", errorMessage: $"Ocorreu um erro ao {(usuario.Ativo ? "desabilitar" : "habilitar")} o usuário, se o erro persistir contate o suporte!"));
		}

		[HttpPost("EnviarLoginPorEmail")]
		public async Task<IActionResult> EnviarLoginPorEmail([FromQuery] int idUsuario)
		{
			var usuario = _serviceAspNetUser.ObterPorId(idUsuario, "Usuario");

			var clientUrl = new Uri(Request.Headers.Referer.FirstOrDefault() ?? Request.GetDisplayUrl());

            byte[] bytes = Convert.FromBase64String(usuario.Usuario.Senha);
            string senha = Encoding.ASCII.GetString(bytes);

            var emailOptions = new EmailOptions
            {
				Subject = "Dados de acesso a sua conta!",
				ToEmail = usuario.Email,
				Template = EmailTemplate.NovoUsuario,
				PlaceHolders = new List<KeyValuePair<string, string>>()
				{
					new KeyValuePair<string, string>("{{NOME}}", usuario.Usuario.Nome),
					new KeyValuePair<string, string>("{{LOGIN}}", usuario.UserName),
					new KeyValuePair<string, string>("{{SENHA}}", senha),
					new KeyValuePair<string, string>("{{URL}}", clientUrl.AbsoluteUri)
				}
			};

			bool sucesso = await _serviceEmail.SendEmail(emailOptions);

			return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao enviar o email com os dados de acesso a conta do usuário!", errorMessage: "Erro ao enviar o email com os dados de acesso a conta do usuário!"));

		}

        [HttpPost("EnviarTodosLoginsPorEmail")]
        public async Task<IActionResult> EnviarTodosLoginsPorEmail()
        {
            var usuarios = _serviceAspNetUser.ObterAtivosPorIdEmpresa(_idEmpresa);

            if (usuarios.Length == 0) return Json(this.CreateResponseObject(false, errorMessage: "Não há usuários ativos cadastrados!"));
			
            var clientUrl = new Uri(Request.Headers.Referer.FirstOrDefault() ?? Request.GetDisplayUrl());

			List<EmailOptions> options = new List<EmailOptions>();

            foreach (var usuario in usuarios)
            {
				var emailOptions = new EmailOptions
				{
					Subject = "Dados de acesso a sua conta!",
					ToEmail = usuario.Email,
					Template = EmailTemplate.NovoUsuario,
					PlaceHolders = new List<KeyValuePair<string, string>>()
				    {
					    new KeyValuePair<string, string>("{{NOME}}", usuario.Usuario.Nome),
					    new KeyValuePair<string, string>("{{LOGIN}}", usuario.UserName),
					    new KeyValuePair<string, string>("{{SENHA}}", usuario.Usuario.Senha),
					    new KeyValuePair<string, string>("{{URL}}", clientUrl.AbsoluteUri)
				    }
				};

				options.Add(emailOptions);

			}

			bool sucesso = await _serviceEmail.SendAllEmails(options.ToArray());

			return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao enviar os emails com os dados de acesso as contas dos usuários!", errorMessage: "Erro ao enviar os emails com os dados de acesso as contas dos usuários!"));

        }
	}
}
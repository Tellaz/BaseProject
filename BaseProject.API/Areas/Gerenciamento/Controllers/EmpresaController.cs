using Microsoft.AspNetCore.Mvc;
using BaseProject.DAO.IService;
using Microsoft.AspNetCore.Authorization;
using BaseProject.DAO.Models.Filters;
using BaseProject.DAO.Models.Views;
using BaseProject.DAO.Models.Others;
using BaseProject.Util;
using System.Security.Claims;

namespace BaseProject.API.Areas.Gerenciamento.Controllers
{
    [Authorize]
    [Area("Gerenciamento")]
	[Route("[area]/[controller]")]
	public class EmpresaController : Controller
	{
        private readonly int _idUsuario;
        private readonly int _idEmpresa;
		private readonly IServiceUsuario _serviceUsuario;
		private readonly IServiceEmpresa _serviceEmpresa;	

		public EmpresaController(
            IHttpContextAccessor httpContextAccessor,
			IServiceUsuario serviceUsuario,
			IServiceEmpresa serviceEmpresa
		)
		{
            _idUsuario = Int32.Parse(httpContextAccessor.HttpContext.User.FindFirstValue("IdUsuario"));
            _idEmpresa = serviceUsuario.ObterIdEmpresaSelecionada(httpContextAccessor.HttpContext);
			_serviceUsuario = serviceUsuario;
            _serviceEmpresa = serviceEmpresa;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("ObterParaSelect")]
        public IActionResult ObterParaSelect()
        {
            var dados = new EmpresasAdmVM
            {
                Empresas = _serviceEmpresa.ObterParaSelect(true),
                IdEmpresaSelecionada = _serviceUsuario.ObterIdEmpresaSelecionada(_idUsuario) ?? 0,
            };

            return Json(this.CreateResponseObject(true, dados));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("EditarEmpresaSelecionada")]
        public IActionResult EditarEmpresaSelecionada([FromQuery] int idEmpresa)
        {
            var usuario = _serviceUsuario.ObterPorId(_idUsuario);

            if (idEmpresa > 0) usuario.IdEmpresaSelecionada = idEmpresa;
            else usuario.IdEmpresaSelecionada = null;

            var sucesso = _serviceUsuario.Editar(usuario);

            return Json(this.CreateResponseObject(sucesso, errorMessage: "Erro ao editar a empresa do usuário ADM"));
        }

        [HttpPost("Listar")]
		public IActionResult Listar([FromBody] DTParam<EmpresaFM> param)
		{
			var result = _serviceEmpresa.Listar(param);

            return Json(this.CreateResponseObject(true, new
            {
                total = result.Total,
                data = result.Itens
            }));
        }

        [HttpPut("Editar")]
		public IActionResult Editar([FromBody] EmpresaVM model)
		{
            var empresa = _serviceEmpresa.ObterPorId(model.Id, "EmpresaLogo");

            empresa.RazaoSocial = model.RazaoSocial;
            empresa.NomeFantasia = model.NomeFantasia;
            empresa.CNPJ = model.CNPJ.Unmask();
            empresa.EmpresaLogo = model.EmpresaLogo;			

            bool sucesso = _serviceEmpresa.Editar(empresa);

			return Json(this.CreateResponseObject(sucesso, successMessage: "Sucesso ao editar a empresa!", errorMessage: "Erro ao editar a empresa!"));
		}

        [HttpGet("Obter")]
        public IActionResult Obter()
        {
            var empresa = _serviceEmpresa.ObterPorId(_idEmpresa, "EmpresaLogo");

			var sucesso = empresa != null;

            return Json(this.CreateResponseObject(sucesso, sucesso ? new EmpresaVM(empresa) : null, errorMessage: "Erro ao obter os dados da empresa!"));
        }
	}
}
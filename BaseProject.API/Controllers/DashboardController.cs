using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.Util;
using System.Text;

namespace BaseProject.API.Controllers
{
    public class DashboardController : Controller
	{
		private UserManager<AspNetUser> _userManager;
		private RoleManager<IdentityRole> _roleManager;
		private readonly IServiceUsuario _serviceUsuario;
		private readonly IServiceEmpresa _serviceEmpresa;

		public DashboardController(
			UserManager<AspNetUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IServiceUsuario serviceUsuario,
			IServiceEmpresa serviceEmpresa
		)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_serviceUsuario = serviceUsuario;
			_serviceEmpresa = serviceEmpresa;
		}
				
		public async Task<IActionResult> Index()
		{
            try
            {
                //Preencha os dados e acesse https://localhost:44301/
                string email = "admin@jptech.com.br"; //Seu email
                string nome = "Admin"; //Seu nome
                string sobrenome = ""; //Seu sobrenome
                string senha = "@Sp230198"; //Sua senha

                bool sucesso = true;

                sucesso = await GerarPermissao();

                if (!sucesso) return Ok("ATENÇÃO: Houve um erro ao adicionar/editar as permissões!");

                sucesso = GerarEmpresa();

                if (!sucesso) return Ok("ATENÇÃO: Houve um erro ao adicionar a empresa principal!");

                var user = new AspNetUser
                {
                    UserName = email.ToLower(),
                    Email = email.ToLower(),
                    TwoFactorEnabled = false,
                    Usuario = new Usuario
                    {
                        IdEmpresa = 1,
                        Nome = nome.Trim(),
                        Sobrenome = string.IsNullOrEmpty(sobrenome) ? null : sobrenome.Trim(),
                        Email = email.ToLower(),
                        Senha = Convert.ToBase64String(Encoding.ASCII.GetBytes(senha)),
                        DataCadastro = DateTime.Now.ToBrasiliaTime(),
                        Ativo = true,
                    }
                };

                var existeUsuario = _serviceUsuario.ExisteEmail(user.Usuario.Email);

                if (!existeUsuario)
                {
                    var chkUser = await _userManager.CreateAsync(user, senha);

                    sucesso = chkUser.Succeeded;

                    if (sucesso)
                    {
                        var roles = new List<string>();

                        roles.Add("Administrador");

                        var chkRole = await _userManager.AddToRolesAsync(user, roles);

                        sucesso = chkRole.Succeeded;
                    }
                }

                if (!sucesso) return Ok("ATENÇÃO: Houve um erro ao adicionar o usuário admin!");

                return Ok("Server is running...");

            }
            catch (Exception e)
            {
                return Ok($"ATENÇÃO: Houve um erro ao adicionar/editar as permissões, empresa principal ou usuário admin! ({e.InnerException?.Message ?? e.Message})");
            }
		}

        private async Task<bool> GerarPermissao()
        {
            bool sucesso = true;

            try
            {
                var roles = new List<IdentityRole>
                {
                    new IdentityRole { Name = "Administrador" },
                };

                foreach(var role in roles)
                {
                    bool roleExists = await _roleManager.RoleExistsAsync(role.Name);

                    if (!roleExists)
                    {
                        var chkRole = await _roleManager.CreateAsync(role);

                        sucesso = chkRole.Succeeded;
                    }
                }
            }
            catch (Exception e)
            {
                sucesso = false;
            }

            return sucesso;
        }

        private bool GerarEmpresa()
        {
            bool sucesso = true;

            try
            {
                var empresa = new Empresa
                {
                    DataCadastro = DateTime.Now.ToBrasiliaTime(),
                    Ativa = true,
                    Dominio = "jptech.com.br",                    
                };

                bool empresaExists = _serviceEmpresa.Existe(empresa.Dominio);

                if (!empresaExists) sucesso = _serviceEmpresa.Adicionar(empresa);
            }
            catch (Exception e)
            {
                sucesso = false;
            }

            return sucesso;
        }


    }
}
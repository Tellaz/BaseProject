using System.Text;

namespace BaseProject.DAO.Models.Views
{
    public class EmpresasAdmVM
    {
		public EmpresaVM[] Empresas { get; set; }  
        public int IdEmpresaSelecionada { get; set; }

        public EmpresasAdmVM() { }
    }
}
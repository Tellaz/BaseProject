using BaseProject.Util;
using System.Text;

namespace BaseProject.DAO.Models.Views.Account
{
    public class AuthVM
    {    
        public UserVM user { get; set; }
        public string[] roles { get; set; }

        public AuthVM() { }

        public AuthVM(UserVM user, string[] roles)
        {
			this.user = user;
            this.roles = roles;
        }

    }
}
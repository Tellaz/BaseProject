using System.ComponentModel.DataAnnotations;

namespace BaseProject.DAO.Models.Views
{
    public class SignUpVM
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Por favor, insira um email válido!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

		[Required(ErrorMessage = "O campo {0} é obrigatório!")]
		[Display(Name = "Sobrenome")]
		public string Sobrenome { get; set; }

		[Required(ErrorMessage = "O campo {0} é obrigatório!")]
		[Display(Name = "Telefone")]
		[StringLength(11, ErrorMessage = "O campo {0} é inválido!")]
		public string Telefone { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]        
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[$*&@#!?%\-\=\\\/\[\]\{\}\(\)])[0-9a-zA-Z$*&@#!?%\-\=\\\/\[\]\{\}\(\)]{8,20}$", ErrorMessage = "A senha deve ter:<br /> <li>Entre 8 e 16 caracteres</li> <li>Pelo menos 1 letra maiúscula e minúscula</li> <li>Pelo menos 1 número e 1 símbolo</li>")]
        public string Password { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        [Display(Name = "Confirme a senha")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "A senha e a confirmação da senha não conferem!")]
        public string ConfirmPassword { get; set; }
        
    }
}

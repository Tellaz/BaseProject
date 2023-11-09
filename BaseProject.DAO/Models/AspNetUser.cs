using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseProject.DAO.Models
{
	public class AspNetUser : IdentityUser
	{
		[InverseProperty("IdAspNetUserNavigation")]
		public virtual Usuario Usuario { get; set; }
	}
}
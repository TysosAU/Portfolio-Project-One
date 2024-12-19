using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class RegisterUser
    {
        [Required]
        [MinLength(1), MaxLength(10)]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Username can only contain letters and numbers with no spaces.")]
        public string Username { get; set; }
        [Required]
        [MinLength(15), MaxLength(100)]
        [RegularExpression(@"[A-Za-z0-9!#$]+", ErrorMessage = "Invalid Password.")]
        public string Password { get; set; }
    }
}

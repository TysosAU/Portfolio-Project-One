using BCrypt.Net;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required]
        [MinLength(1), MaxLength(10)]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Username can only contain letters and numbers with no spaces.")]
        public string UserName { get; set; }
        [Required]
        [MinLength(15), MaxLength(100)]
        [RegularExpression(@"[A-Za-z0-9!#$]+", ErrorMessage = "Invalid Password.")]
        public string Password { get; set; }
        [Required]
        [MinLength(1), MaxLength(15)]
        [RegularExpression(@"^(Administrator|user)$", ErrorMessage = "Invalid Role")]
        public string Role { get; set; }

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, this.Password);
        }
    }
}

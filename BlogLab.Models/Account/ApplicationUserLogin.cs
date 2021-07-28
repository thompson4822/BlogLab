using System.ComponentModel.DataAnnotations;

namespace BlogLab.Models.Account
{
    public class ApplicationUserLogin
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(5, ErrorMessage = "Must be at least 5 characters")]
        [MaxLength(20, ErrorMessage = "Must be no more than 20 characters")]
        public string Username { get; set; }
        
        [Required]
        [MinLength(10, ErrorMessage = "Must be at least 10 characters")]
        [MaxLength(50, ErrorMessage = "Must be no more than 50 characters")]
        public string Password { get; set; }
    }
}
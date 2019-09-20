using System.ComponentModel.DataAnnotations;

namespace DattingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string username  { get; set; }

        [Required]
        [StringLength(8, ErrorMessage = "The password must be at least {0} characters", MinimumLength = 4)]
        public string password { get; set; }
    }
}
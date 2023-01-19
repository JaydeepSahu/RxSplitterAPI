using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Common
{
    public class Login
    {
        [Required]
        //[StringLength(80, MinimumLength = 6, ErrorMessage = "UserName length much be between 6 and 80 characters in length.")]
        //[MaxLength(), RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid email address")]
        public string UserName { get; set; }

        [Required]
        //[StringLength(16, MinimumLength = 8, ErrorMessage = "Password length much be between 8 and 16 characters in length.")]
        //[MaxLength(), RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Please enter a valid password")]
        public string Password { get; set; }
    }
}

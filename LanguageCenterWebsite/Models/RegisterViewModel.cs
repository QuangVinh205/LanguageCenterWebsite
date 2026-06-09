using System.ComponentModel.DataAnnotations;

namespace LanguageCenterWebsite.Models
{
    // Used for the Registration Form (Guest)
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        // NEW: Field for selecting role (Student or Teacher)
        [Required(ErrorMessage = "Please select a role.")]
        [Display(Name = "Register As")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
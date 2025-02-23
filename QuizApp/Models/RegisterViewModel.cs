using QuizApp.Resources;
using System.ComponentModel.DataAnnotations;
namespace QuizApp.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredAttribute_ValidationError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxLengthAttribute_ValidationError")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredAttribute_ValidationError")]
        [StringLength(100, MinimumLength = 6, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLengthAttribute_ValidationError")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "CompareAttribute_ValidationError")]
        public string ConfirmPassword { get; set; }
    }
}
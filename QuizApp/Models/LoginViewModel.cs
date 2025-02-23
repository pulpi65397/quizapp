using QuizApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredAttribute_ValidationError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "MaxLengthAttribute_ValidationError")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredAttribute_ValidationError")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }
}
using QuizApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class JoinQuizViewModel
    {
        public string Token { get; set; }
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "RequiredAttribute_ValidationError")]
        public string Nick { get; set; }

        public int QuizId { get; set; }
    }
}
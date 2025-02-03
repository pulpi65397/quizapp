using Microsoft.AspNetCore.Identity;
using QuizApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Nick { get; set; }
}

using Xunit;
using QuizApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class UzytkownikModelTests
{
    [Fact]
    public void Uzytkownik_NickIsRequired_ValidationFails()
    {
        // Arrange
        var uzytkownik = new Uzytkownik { Nick = null };
        var context = new ValidationContext(uzytkownik);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(uzytkownik, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage.Contains("Nick"));
    }
}
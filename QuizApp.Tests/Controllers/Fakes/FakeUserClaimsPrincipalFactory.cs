using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuizApp.Tests.Controllers.Fakes
{
    internal class FakeUserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
        where TUser : class
    {
        public Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));

            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(principal);
        }
    }
}
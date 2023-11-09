using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BaseProject.DAO.Models;

namespace BaseProject.Test.Mocks
{
    public class SignInManagerMock : SignInManager<AspNetUser>
    {
        public SignInManagerMock()
                : base(new UserManagerMock(),
                     new Mock<IHttpContextAccessor>().Object,
                     new Mock<IUserClaimsPrincipalFactory<AspNetUser>>().Object,
                     new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<AspNetUser>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object,
                     new Mock<IUserConfirmation<AspNetUser>>().Object)
        { }
    }
}

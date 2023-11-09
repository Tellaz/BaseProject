using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BaseProject.DAO.Models;

namespace BaseProject.Test.Mocks
{
    public class UserManagerMock : UserManager<AspNetUser>
    {
        public UserManagerMock()
            : base(new Mock<IUserStore<AspNetUser>>().Object,
              new Mock<IOptions<IdentityOptions>>().Object,
              new Mock<IPasswordHasher<AspNetUser>>().Object,
              new IUserValidator<AspNetUser>[0],
              new IPasswordValidator<AspNetUser>[0],
              new Mock<ILookupNormalizer>().Object,
              new Mock<IdentityErrorDescriber>().Object,
              new Mock<IServiceProvider>().Object,
              new Mock<ILogger<UserManager<AspNetUser>>>().Object)
        { }

    }
}

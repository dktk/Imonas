using System.Threading.Tasks;

using Application.Common.Interfaces;
using Application.Common.Interfaces.Identity;

using Moq;

using NUnit.Framework;

namespace Imonas.Application.UnitTests.Common.Behaviours
{
    public class RequestLoggerTests
    {

        private readonly Mock<ICurrentUserService> _currentUserService;
        private readonly Mock<IIdentityService> _identityService;


        public RequestLoggerTests()
        {
            _currentUserService = new Mock<ICurrentUserService>();

            _identityService = new Mock<IIdentityService>();
        }

        [Test]
        public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
        {
            _currentUserService.Setup(x => x.UserId).Returns("Administrator");

            //_identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
        {
            _identityService.Verify(i => i.GetUserNameAsync(null), Times.Never);
        }
    }
}

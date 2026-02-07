#define TESTING

using NUnit.Framework;
using System.Threading.Tasks;

namespace Imonas.Application.IntegrationTests
{
    using static Testing;

    public class TestBase
    {
        [SetUp]
        public async Task TestSetUp()
        {
            await ResetState();
        }
    }
}

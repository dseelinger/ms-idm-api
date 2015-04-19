using System.Threading.Tasks;
using IdmApi.Controllers;
using IdmApi.DAL.Fakes;
using IdmNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdmApi.Tests
{
    [TestClass]
    public class ResourceControllerTests
    {
        [TestMethod]
        public void It_has_a_CTOR_that_takes_a_repo()
        {
            var repo = new StubIRepository();

            var it = new ResourcesController(repo);
        }

        [TestMethod]
        public async Task It_can_GetById_with_no_select()
        {
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) => Task.FromResult(new IdmResource())
            };

            var it = new ResourcesController(repo);

            var result = await it.GetById("foo");
        }

        [TestMethod]
        public async Task It_can_GetById_with_select()
        {
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) =>
                {
                    Assert.AreEqual("bar", strings[0]);
                    Assert.AreEqual("bat", strings[1]);
                    return Task.FromResult(new IdmResource());
                }
            };

            var it = new ResourcesController(repo);

            var result = await it.GetById("foo", "bar,bat");
        }

        [TestMethod]
        public async Task It_can_GetById_with_sloppy_select()
        {
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) =>
                {
                    Assert.AreEqual("bar", strings[0]);
                    Assert.AreEqual("bat", strings[1]);
                    return Task.FromResult(new IdmResource());
                }
            };

            var it = new ResourcesController(repo);

            var result = await it.GetById("foo", " bar, bat ");
        }
    }
}

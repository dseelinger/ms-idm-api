using IdmNet.SoapModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace IdmApi.Tests.Spikes
{
    [TestClass]
    public class JsonSpikes
    {
        [TestMethod]
        public void ChangesJsonSpike()
        {
            var changes1 = new[]
            {
                new Change(ModeType.Replace, "FirstName", "FirstNameTest"),
                new Change(ModeType.Replace, "LastName", "LastNameTest"),
                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab1.lab"),
                new Change(ModeType.Delete, "ProxyAddressCollection", "joe@lab2.lab"),
            };

            var json = JsonConvert.SerializeObject(changes1);
        }
    }
}

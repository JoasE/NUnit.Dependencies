using NUnit.Dependencies.Tests.PingPong.Async;
using NUnit.Dependencies.Tests.PingPong.Sync;
using NUnit.Framework;

namespace NUnit.Dependencies.Tests.PingPong.Faulty
{
    public class FaultyTests
    {
        [Test]
        public void Sync()
        {
            // Arrange
            var pinger = new Ping
            {
                Param = "notping"
            };

            // Execute and Validate
            Assert.Throws<IgnoreException>(() =>
            {
                var result = pinger.Result;
            });
        }

        [Test]
        public void Async()
        {
            // Arrange
            var pinger = new AsyncPing()
            {
                Param = "notping"
            };

            // Execute and Validate
            Assert.Throws<IgnoreException>(() =>
            {
                var result = pinger.Result;
            });
        }
    }
}

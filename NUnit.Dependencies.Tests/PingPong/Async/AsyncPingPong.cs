using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.Dependencies.Tests.PingPong.Async
{
    public class AsyncPing : AsyncDependencyFixture<string>
    {
        public string Param { get; set; } = "ping";

        protected override async Task<string> TestHappyPath()
        {
            // Act
            var result = await Task.Run(() => Param);

            // Assert
            Assert.AreEqual("ping", result);

            return result;
        }
    }

    public class AsyncPong : AsyncDependencyFixture<string>
    {
        private readonly AsyncPing ping;

        public AsyncPong(AsyncPing ping)
        {
            this.ping = ping;
        }

        protected override async Task<string> TestHappyPath()
        {
            // Act
            var result = await Task.Run(() => ping.Result + "pong");

            // Assert
            Assert.AreEqual("pingpong", result);

            return result;
        }
    }
}

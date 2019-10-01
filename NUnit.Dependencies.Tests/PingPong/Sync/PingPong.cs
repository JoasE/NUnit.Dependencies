using NUnit.Framework;

namespace NUnit.Dependencies.Tests.PingPong.Sync
{
    public class Ping : DependencyFixture<string>
    {
        public string Param { get; set; } = "ping";

        protected override string TestHappyPath()
        {
            // Assert
            Assert.AreEqual("ping", Param);

            // Return
            return Param;
        }
    }

    public class Pong : DependencyFixture<string>
    {
        private readonly Ping ping;

        public Pong(Ping ping)
        {
            this.ping = ping;
        }

        protected override string TestHappyPath()
        {
            // Act
            var result = ping.Result + "pong";

            // Assert
            Assert.AreEqual("pingpong", result);

            // Return
            return result;
        }
    }
}

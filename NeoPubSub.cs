using System;
using System.IO;
using ServiceStack.Redis;

namespace Neo.Plugins
{
    public class NeoPubSub : Plugin, ILogPlugin
    {
        private readonly RedisClient connection;

        public override string Name => "NeoPubSub";

        public NeoPubSub()
        {
            Console.WriteLine($"Connecting to NeoPubSub server at {Settings.Default.Host}:{Settings.Default.Port}");
            this.connection = new RedisClient($"{Settings.Default.Host}:{Settings.Default.Port}");
            if (this.connection == null) {
                Console.WriteLine($"Connection failed!");
            } else {
                Console.WriteLine($"Connected.");
                System.ActorSystem.ActorOf(Publisher.Props(System.Blockchain, this.connection));
                Log("NeoPubSub Plugin", LogLevel.Info, "Ready");
            }
        }

        public void Log(string source, LogLevel level, string message)
        {
            connection.PublishMessage(source, message);
        }

        public override void Configure()
        {
            Settings.Load(GetConfiguration());
        }
    }
}

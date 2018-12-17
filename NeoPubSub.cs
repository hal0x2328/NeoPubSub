using System;
using System.IO;
using StackExchange.Redis;

namespace Neo.Plugins
{
    public class NeoPubSub : Plugin, ILogPlugin
    {
        private readonly ConnectionMultiplexer connection;

        public override string Name => "NeoPubSub";

        public NeoPubSub()
        {
            Console.WriteLine($"Connecting to PubSub server at {Settings.Default.RedisHost}:{Settings.Default.RedisPort}");
            this.connection = ConnectionMultiplexer.Connect($"{Settings.Default.RedisHost}:{Settings.Default.RedisPort}");
            if (this.connection == null) {
                Console.WriteLine($"Connection failed!");
            } else {
                Console.WriteLine($"Connected.");
                System.ActorSystem.ActorOf(Publisher.Props(System.Blockchain, this.connection));
                Log("NeoPubSub Plugin", LogLevel.Info, "Ready");
            }
        }

        void ILogPlugin.Log(string source, LogLevel level, string message)
        {
            connection.GetSubscriber().Publish(source, message);
        }

        public override void Configure()
        {
            Console.WriteLine("Loading configuration for NeoPubSub plugin...");
            Settings.Load(GetConfiguration());
        }

    }
}

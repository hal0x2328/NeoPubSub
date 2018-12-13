using Microsoft.Extensions.Configuration;
using Neo.Network.P2P;
using System.Reflection;

namespace Neo.Plugins
{
    internal class Settings
    {
        public string Host { get; }
        public string Port { get; }

        public static Settings Default { get; private set; }

        private Settings(IConfigurationSection section)
        {
            this.Host = string.Format(section.GetSection("Host").Value, Message.Magic.ToString("X8"));
            this.Port = string.Format(section.GetSection("Port").Value, Message.Magic.ToString("X8"));
        }
        public static void Load(IConfigurationSection section)
        {
            Default = new Settings(section);
        }
    }
}

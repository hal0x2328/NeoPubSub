using Akka.Actor;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.VM;
using Neo.SmartContract;
using StackExchange.Redis;
using System;

namespace Neo.Plugins
{
    public class Publisher : UntypedActor
    {
        private readonly ConnectionMultiplexer connection;

        public Publisher(IActorRef blockchain, ConnectionMultiplexer connection)
        {
            this.connection = connection;
            blockchain.Tell(new Blockchain.Register());
            foreach(string c in Settings.Default.WatchContracts)
            {
                Console.WriteLine("Watching contract " + c);
            }
        }

        protected override void OnReceive(object message)
        {
            if (message is Blockchain.ApplicationExecuted e)
            {
                JObject json = new JObject();
                string txid = e.Transaction.Hash.ToString();
                foreach (ApplicationExecutionResult p in e.ExecutionResults)
                {
                    if (p.VMState.ToString() == "HALT, BREAK")
                    {
                        foreach(NotifyEventArgs q in p.Notifications)
                        {
                            string contract = q.ScriptHash.ToString();
                            if (Array.IndexOf(Settings.Default.WatchContracts, contract) >= 0 || 
                                Array.IndexOf(Settings.Default.WatchContracts, "*") >= 0)
                            {
                                string r = q.State.ToParameter().ToJson().ToString();
                                connection.GetSubscriber().Publish(contract, $"{txid} {r}");
                            }
                        }
                    }
                }
            }
        }

        public static Props Props(IActorRef blockchain, ConnectionMultiplexer connection)
        {
            return Akka.Actor.Props.Create(() => new Publisher(blockchain, connection));
        }

    }
}

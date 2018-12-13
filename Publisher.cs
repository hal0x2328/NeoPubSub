using Akka.Actor;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.VM;
using Neo.SmartContract;
using ServiceStack.Redis;

namespace Neo.Plugins
{
    public class Publisher : UntypedActor
    {
        private readonly RedisClient connection;

        public Publisher(IActorRef blockchain, RedisClient connection)
        {
            this.connection = connection;
            blockchain.Tell(new Blockchain.Register());
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
                            string r = q.State.ToParameter().ToJson().ToString();
                            connection.PublishMessage(contract, $"{txid} {r}");
                        }
                    }
                }
            }
        }

        public static Props Props(IActorRef blockchain, RedisClient connection)
        {
            return Akka.Actor.Props.Create(() => new Publisher(blockchain, connection));
        }

    }
}

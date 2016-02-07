using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Lidgren.Network;

using Common;

public class Kludge
{
    private static volatile bool shutdown = false;
    private static NetworkManager network = null;
    private static MessageHandler handler = null;
    private static Recommender recommender = null;

    private static void OnConnected(
        object sender,
        NetConnection connection,
        string output)
    {
        Console.WriteLine(
            "Connected: " + connection.RemoteUniqueIdentifier);
    }

    private static void OnDisconnected(
        object sender,
        NetConnection connection,
        string output)
    {
        Console.WriteLine(
            "Disconnected: " + connection.RemoteUniqueIdentifier);
    }

    private static void OnOtherStatus(
        object sender,
        NetConnection connection,
        string output)
    {
        Console.WriteLine(output);
    }

    private static void EncodeRecommendations(
        List<Candidate> candidates,
        NetOutgoingMessage outMsg)
    {
        outMsg.Write((byte)MsgType.RecommendationContent);
        outMsg.Write(candidates.Count);

        foreach (Candidate candidate in candidates)
        {
            TransitionEvent evt = candidate.Path[0].Events[0];
            string name = evt.Descriptor.Name;
            uint[] participants = evt.Participants;

            outMsg.Write(name);
            outMsg.Write(participants.Length);
            foreach (uint participant in participants)
                outMsg.Write(participant);
        }
    }

    public static void RecommendationRequested(NetIncomingMessage inMsg)
    {
        Console.WriteLine("Recommendation requested...");

        if (recommender.GetBusy().Count() == 0)
        {
            List<Candidate> candidates =
                new List<Candidate>(recommender.GetCandidates());
            NetOutgoingMessage outMsg = network.CreateMessage();
            EncodeRecommendations(candidates, outMsg);
            network.Send(outMsg);
            Console.WriteLine("Sent recommendations.");
        }
        else
        {
            Console.WriteLine("Holding recommendations until all objects are free...");
        }
    }

    public static void RecommendationSelected(NetIncomingMessage inMsg)
    {
        int selected = inMsg.ReadInt32();
        int count = inMsg.ReadInt32();
        for (int i = 0; i < count; i++)
            recommender.AddBusy(inMsg.ReadUInt32());

        Console.WriteLine("Recommendation selected (" + selected + ")");

        string busyOutput = "Currently Busy:";
        foreach (uint id in recommender.GetBusy())
            busyOutput += " " + id;
        Console.WriteLine(busyOutput);

        recommender.CandidateSelected(selected);
    }

    public static void ObjectsFreed(NetIncomingMessage inMsg)
    {
        int count = inMsg.ReadInt32();
        for (int i = 0; i < count; i++)
            recommender.RemoveBusy(inMsg.ReadUInt32());

        Console.WriteLine("Freed " + count + " objects");

        string busyOutput = "Currently Busy:";
        foreach (uint id in recommender.GetBusy())
            busyOutput += " " + id;
        Console.WriteLine(busyOutput);
    }

    public static void SentimentsSent(NetIncomingMessage inMsg)
    {
      int countWant = inMsg.ReadInt32();
      string[] want = new string[countWant];
      for (int i = 0; i < countWant; i++)
        want[i] = inMsg.ReadString();

      int countDont = inMsg.ReadInt32();
      string[] dont = new string[countDont];
      for (int i = 0; i < countDont; i++)
        dont[i] = inMsg.ReadString();

      Console.WriteLine("Want: " + string.Join(", ", want));
      Console.WriteLine("Don't Want: " + string.Join(", ", dont));

      GoalSelector.SentimentWant = want;
      GoalSelector.SentimentDont = dont;
    }

    static void Main(string[] args)
    {
        ExplorationSpace space = Common.ExternalIO.LoadSpace(
            ExternalIO.WorldDataPath(),
            ExternalIO.GraphDataPath());
        recommender = new Recommender(space);

        if (space != null)
        {
            Console.WriteLine("Starting server...");
            Task serverTask = StartServer();

            Console.WriteLine("Press ENTER to stop server . . .");
            Console.ReadLine();

            shutdown = true;
            serverTask.Wait();
        }

        Console.WriteLine("Press any key to continue . . .");
        Console.ReadKey();
    }

    private static Task StartServer()
    {
        NetPeerConfiguration config = new NetPeerConfiguration("Kludge");
        config.MaximumConnections = 100;
        config.Port = 14343;
        config.AutoFlushSendQueue = true;
        NetPeer peer = new NetServer(config);
        network = new NetworkManager(peer);

        network.Connected += OnConnected;
        network.Disconnected += OnDisconnected;
        network.OtherStatus += OnOtherStatus;

        handler = new MessageHandler(network);
        handler.RecommendationRequested += RecommendationRequested;
        handler.RecommendationSelected += RecommendationSelected;
        handler.ObjectsFreed += ObjectsFreed;
        handler.Sentiments += SentimentsSent;

        Task serverTask = new Task(() => RunServer(network));
        serverTask.Start();
        return serverTask;
    }

    private static void RunServer(NetworkManager network)
    {
        network.Start();
        while (shutdown == false)
            network.Update();
        network.Shutdown("Bye! (Server Shutdown)");
        Console.WriteLine("Server shut down successfully.");
    }

    private static void ProcessLine(StreamWriter writer, string line)
    {
        Console.WriteLine("> " + line);
        writer.WriteLine(line);
    }
}
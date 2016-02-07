using UnityEngine;

using System;
using System.Threading;
using System.Collections.Generic;

using Lidgren.Network;

public class NetworkClient : MonoBehaviour
{
    public NetEventRunner Runner;

    public bool Connected = false;

    private MessageHandler handler;
    public MessageHandler Handler { get { return this.handler; } }

    private NetworkManager network;
    private NetClient client;

    public NetOutgoingMessage CreateMessage()
    {
        if (this.Connected == true)
            return this.network.CreateMessage();
        return null;
    }

    public void SendMessage(NetOutgoingMessage msg)
    {
        if (this.Connected == true)
        {
            Debug.Log("Sending...");
            this.network.Send(msg);
        }
    }

    public void RecommendationReceived(NetIncomingMessage inMsg)
    {
        Debug.Log("RecommendationReceived");

        int count = inMsg.ReadInt32();
        string[] names = new string[count];
        uint[][] participants = new uint[count][];

        for (int i = 0; i < count; i++)
        {
            names[i] = inMsg.ReadString();
            int numParticipants = inMsg.ReadInt32();
            participants[i] = new uint[numParticipants];
            for (int j = 0; j < numParticipants; j++)
                participants[i][j] = inMsg.ReadUInt32();
        }

        this.Runner.ReceivedCandidates(names, participants);
    }

    void Start()
    {
        NetPeerConfiguration config = new NetPeerConfiguration("Kludge");
        config.AutoFlushSendQueue = true;
        network = new NetworkManager(new NetClient(config));
        this.handler = new MessageHandler(network);
        this.handler.RecommendationReceived += this.RecommendationReceived;

        network.Connected += OnConnected;
        network.Disconnected += OnDisconnected;
        network.OtherStatus += OnOtherStatus;

#if UNITY_WEBPLAYER
        Security.PrefetchSocketPolicy(PlayerState.ipAddress, 14343);
#endif
        Connect("localhost", 14343);
    }

    void Update()
    {
        this.network.Update();
    }

    void OnDisable()
    {
        this.network.Shutdown("Bye! (Unity Shutdown)");
    }

    public void Connect(string host, int port)
    {
        this.network.Start();
        // Send universal greeting
        NetOutgoingMessage handshake = network.CreateMessage();
        handshake.Write("ba-weep-gra-na-weep-ninny-bong");
        this.network.Connect(host, port, handshake);
    }

    public void Shutdown()
    {
        this.network.Disconnect("Bye! (Unity Disconnect)");
    }

    public void OnConnected(object sender, NetConnection connection, string output)
    {
        UnityEngine.Debug.Log("Connected: " + connection.RemoteUniqueIdentifier);
        this.Connected = true;
        this.Runner.OnConnected();
    }

    public void OnDisconnected(object sender, NetConnection connection, string output)
    {
        this.Connected = false;
        UnityEngine.Debug.Log("Disconnected: " + connection.RemoteUniqueIdentifier);
    }

    public void OnOtherStatus(object sender, NetConnection connection, string output)
    {
        UnityEngine.Debug.Log(output);
    }
}

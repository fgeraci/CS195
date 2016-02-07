using System;
using System.Collections;
using System.Collections.Generic;

using Lidgren.Network;

/// <summary>
/// The main workhorse for network traffic, handles establishing 
/// connection(s) with peers and responds to events from net traffic.
/// Needs to be regularly updated to read incoming messages.
/// </summary>
public class NetworkManager
{
#if EXTERNAL
    private static void Log(object message)
    {
        Console.WriteLine(message);
    }

    private static void LogWarning(object message)
    {
        Console.WriteLine("WARNING: " + message);
    }
#else
    private static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    private static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }
#endif


    public delegate void NetworkEvent(object sender);
    public delegate void ConnectionEvent(
        object sender, 
        NetConnection connection, 
        string output = null);
    public delegate void DataEvent(
        object sender,
        NetIncomingMessage msg);

    public event NetworkEvent Started;
    public event NetworkEvent Stopped;

    public event ConnectionEvent Connected;
    public event ConnectionEvent Disconnected;
    public event ConnectionEvent OtherStatus;

    public event DataEvent ReceivedData;

    private readonly NetPeer peer = null;
    private readonly Dictionary<long, NetConnection> connections;

    public NetPeerStatus Status { get { return this.peer.Status; } }
    public long LocalUID { get { return this.peer.UniqueIdentifier; } }

    /// <summary>
    /// Instantiates the Network with its own NetPeer for traffic
    /// </summary>
    public NetworkManager(NetPeer peer)
    {
        this.peer = peer;
        this.connections = new Dictionary<long, NetConnection>();
    }

    /// <summary>
    /// Start the connection
    /// </summary>
    public void Start()
    {
        this.peer.Start();
        if (this.Started != null)
            this.Started.Invoke(this);
    }

    /// <summary>
    /// Shutdown the connection
    /// </summary>
    public void Shutdown(string reason)
    {
        this.peer.Shutdown(reason);
        if (this.Stopped != null)
            this.Stopped.Invoke(this);
    }

    /// <summary>
    /// Connect to a peer
    /// </summary>
    public void Connect(string host, int port, NetOutgoingMessage hail)
    {
        this.peer.Connect(host, port, hail);
    }

    /// <summary>
    /// Disconnect from a peer (for use with a NetClient only!)
    /// </summary>
    public void Disconnect(string reason)
    {
        ((NetClient)this.peer).Disconnect(reason);
    }

    /// <summary>
    /// Creates a message for our peer
    /// </summary>
    public NetOutgoingMessage CreateMessage()
    {
        return this.peer.CreateMessage();
    }

    /// <summary>
    /// Handles incoming data from any peer
    /// </summary>
    private void HandleData(NetIncomingMessage msg)
    {
        if (this.ReceivedData != null)
            this.ReceivedData(this, msg);
    }

    /// <summary>
    /// Sends a message from a particular relay to a single recipient
    /// </summary>
    public void Send(
        NetOutgoingMessage msg,
        NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
    {
        // TODO: [EFFICIENCY] Use some real sequence numbers
        List<NetConnection> targets = 
            new List<NetConnection>(this.connections.Values);
        peer.SendMessage(msg, targets, method, 0);
    }

    /// <summary>
    /// Sends a message from a particular relay to a single recipient
    /// </summary>
    public void Send(
        NetConnection target,
        NetOutgoingMessage msg,
        NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
    {
        // TODO: [EFFICIENCY] Use some real sequence numbers
        if (target != null)
            peer.SendMessage(msg, target, method, 0);
    }

    /// <summary>
    /// Sends a message from a particular relay to a number of recipients
    /// </summary>
    public void Send(
        IList<NetConnection> targets, 
        NetOutgoingMessage msg,
        NetDeliveryMethod method = NetDeliveryMethod.ReliableOrdered)
    {
        // TODO: [EFFICIENCY] Use some real sequence numbers
        if (targets.Count > 0)
            peer.SendMessage(msg, targets, method, 0);
    }

    /// <summary>
    /// Called when we've established connection with a peer
    /// </summary>
    private void OnConnect(NetConnection connection)
    {
        long netId = connection.RemoteUniqueIdentifier;
        this.connections[netId] = connection;
        if (this.Connected != null)
            this.Connected.Invoke(this, connection);
    }

    /// <summary>
    /// Called when a peer has disconnected
    /// </summary>
    private void OnDisconnect(NetConnection connection)
    {
        long netId = connection.RemoteUniqueIdentifier;
        this.connections.Remove(netId);
        if (this.Disconnected != null)
            this.Disconnected.Invoke(this, connection);
    }

    /// <summary>
    /// Called when a peer has changed to some other status
    /// </summary>
    private void OnOtherStatus(NetConnection connection, string output)
    {
        if (this.OtherStatus != null)
            this.OtherStatus.Invoke(this, connection, output);
    }

    /// <summary>
    /// Handles messages from peers regarding a change in their
    /// connection status
    /// </summary>
    private void OnStatusChanged(NetIncomingMessage inMsg)
    {
        long netId =
            inMsg.SenderConnection.RemoteUniqueIdentifier;
        NetConnectionStatus status =
            (NetConnectionStatus)inMsg.ReadByte();
        if (status == NetConnectionStatus.Connected)
            this.OnConnect(inMsg.SenderConnection);
        else if (status == NetConnectionStatus.Disconnected)
            this.OnDisconnect(inMsg.SenderConnection);
        else
        {
            string reason = inMsg.ReadString();
            this.OnOtherStatus(
                inMsg.SenderConnection,
                NetUtility.ToHexString(netId)
                + " " + status + ": " + reason);
        }
    }

    /// <summary>
    /// Periodic update, handles incoming messages and connect
    /// or disconnect status changes
    /// </summary>
    public void Update()
    {
        NetIncomingMessage inMsg;
        while ((inMsg = peer.ReadMessage()) != null)
        {
            switch (inMsg.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    string text = inMsg.ReadString();
                    Log(text);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    this.OnStatusChanged(inMsg);
                    break;
                case NetIncomingMessageType.Data:
                    HandleData(inMsg);
                    break;
                default:
                    LogWarning(
                        "Unhandled type: " + inMsg.MessageType
                        + " " + inMsg.LengthBytes + " bytes "
                        + inMsg.DeliveryMethod + "|"
                        + inMsg.SequenceChannel);
                    break;
            }
        }
    }
}

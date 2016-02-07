using System;
using System.Collections;

using Lidgren.Network;

public class MessageHandler
{
    public delegate void MessageEvent(NetIncomingMessage msg);

    public event MessageEvent RecommendationRequested;
    public event MessageEvent RecommendationReceived;
    public event MessageEvent RecommendationSelected;
    public event MessageEvent ObjectsFreed;
    public event MessageEvent Sentiments;

    public MessageHandler(NetworkManager manager)
    {
        manager.ReceivedData += this.ReceiveMessage;
    }

    public void ReceiveMessage(object sender, NetIncomingMessage msg)
    {
#if !EXTERNAL
        UnityEngine.Debug.Log("Got message [" + msg.LengthBytes + "B]");
#else
        Console.WriteLine("Got message [" + msg.LengthBytes + "B]");
#endif
        MsgType type = (MsgType)msg.ReadByte();
        switch (type)
        {
            case MsgType.RecommendationRequest:
                if (this.RecommendationRequested != null)
                    this.RecommendationRequested(msg);
                break;
            case MsgType.RecommendationContent:
                if (this.RecommendationReceived != null)
                    this.RecommendationReceived(msg);
                break;
            case MsgType.RecommendationSelected:
                if (this.RecommendationSelected != null)
                    this.RecommendationSelected(msg);
                break;
            case MsgType.ObjectsFreed:
                if (this.ObjectsFreed != null)
                    this.ObjectsFreed(msg);
                break;
            case MsgType.Sentiments:
                if (this.Sentiments != null)
                    this.Sentiments(msg);
                break;
            default: break;
        }
    }
}
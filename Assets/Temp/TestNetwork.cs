using System;
using System.Threading;
using Core.Scripts.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class TestNetwork : MonoBehaviour
{
  [SerializeField] NetworkConfig networkConfig;
  [SerializeField] bool isOn;
  [ContextMenu("Start Client")]
  private void client()
  {
    isOn = true;
    EventBasedNetListener listener = new EventBasedNetListener();
    NetManager client = new NetManager(listener);
    client.Start();
    client.Connect(networkConfig.IpAddress, networkConfig.HostPort, "SomeConnectionKey" /* text key or NetDataWriter */);
    listener.NetworkReceiveEvent += OnReceive;

    while (isOn)
    {
      client.PollEvents();
      Thread.Sleep(15);
    }

    client.Stop();
  }

  private void OnReceive(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
  {
    Debug.Log($"We got: {dataReader.GetString(100 /* max length of string */)}");
    dataReader.Recycle();
  }

  [ContextMenu("Start Server")]
  private void Server()
  {
    isOn = true;
    EventBasedNetListener listener = new EventBasedNetListener();
    NetManager server = new NetManager(listener);
    server.Start(networkConfig.HostPort);

    listener.ConnectionRequestEvent += request =>
    {
      if(server.GetPeersCount(ConnectionState.Any) < 10 /* max connections */)
        request.AcceptIfKey("SomeConnectionKey");
      else
        request.Reject();
    };

    listener.PeerConnectedEvent += peer =>
    {
      Console.WriteLine("We got connection: {0}", peer.Address); // Show peer ip
      NetDataWriter writer = new NetDataWriter();                 // Create writer class
      writer.Put("Hello client!");                                // Put some string
      peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
    };

    while (isOn)
    {
      server.PollEvents();
      Thread.Sleep(15);
    }
    server.Stop();
  }

  private void OnApplicationQuit()
  {
    isOn = false;
  }
}
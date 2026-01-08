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
  private bool _didClientConnectToServer;
  int _tick = 0;
  [ContextMenu("Start Client")]
  private void client()
  {
    isOn = true;
    EventBasedNetListener listener = new EventBasedNetListener();
    NetManager client = new NetManager(listener);
    client.Start();
    client.Connect(networkConfig.IpAddress, networkConfig.HostPort, "SomeConnectionKey" /* text key or NetDataWriter */);
    listener.NetworkReceiveEvent += OnClientReceive;

    _ = StartClientTick(client);
  }
  
  private async Awaitable StartClientTick(NetManager client)
  {
    while (isOn)
    {
      client.PollEvents();
      await Awaitable.WaitForSecondsAsync(0.015f);
    }
    client.Stop();
  }

  private void OnClientReceive(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
  {
    Debug.Log($"We got: {dataReader.GetString(100 /* max length of string */)}");
    dataReader.Recycle();

    if (!_didClientConnectToServer)
    {
      _didClientConnectToServer = true;
      _ = StartSendingEveryTickToServer(fromPeer);
    }
  }

  private async Awaitable StartSendingEveryTickToServer(NetPeer peer)
  {
    Debug.Log("start sending every tick to server");
    while (isOn)
    {
      NetDataWriter writer = new NetDataWriter();                 // Create writer class
      
      writer.Put($"Hello server! {_tick++}");                                // Put some string
      peer.Send(writer, DeliveryMethod.Unreliable); 
      await Awaitable.WaitForSecondsAsync(0.015f);
    }
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
      Debug.Log($"We got connection: {peer.Address}"); // Show peer ip
      NetDataWriter writer = new NetDataWriter();                 // Create writer class
      writer.Put("Hello client!");                                // Put some string
      peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
    };

    listener.NetworkReceiveEvent+= OnServerReceive;
    _ = StartServerTick(server);
  }

  private void OnServerReceive(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
  {
    var str = dataReader.GetString(100 /* max length of string */);
    Debug.Log($"We got: {str}");
    dataReader.Recycle();
    NetDataWriter writer = new NetDataWriter();        
    writer.Put($"Server got: {str}");                       
    fromPeer.Send(writer, DeliveryMethod.Unreliable); 
  }
  
  private async Awaitable StartServerTick(NetManager server)
  {
    while (isOn)
    {
      server.PollEvents();
      await Awaitable.WaitForSecondsAsync(0.015f);
    }
    server.Stop();
  }

  private void OnApplicationQuit()
  {
    isOn = false;
  }
}
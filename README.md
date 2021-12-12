# MulticastLib
 Library for joining and listening to a multicast group
## What is Multicast
 Multicast is a one-to-many form of network communication on a local network. In order to multicast the client first has to join a multicast group. Once joined the client can send a message to the group and any other clients joined to the same group will receive the message.
## How to use MulticastLib
MulticastLib simplifies the underlying mechanisms of joining and sending/receiving messages on a multicast group.
### Basic Example
```C#
using MulticastLib;
using MulticastLib.Interfaces;
using MulticastLib.Models;
using System.Net;

var multicast = new NetworkManager();
multicast.MessageReceived += Multicast_MessageReceived;
var localIP = NetworkUtilities.GetFastestInterfaceAddress();
var multicastGroupIP = IPAddress.Parse("239.255.0.1");
multicast.StartListener(localIP, multicastGroupIP);

Console.WriteLine($"Connected to {multicastGroupIP} on {localIP}");

await Task.Run(async () => 
{ 
    while(true)
    {
        var message = new Message(localIP.ToString(), Console.ReadLine());
        await multicast.SendMessage(message);
    }
});

void Multicast_MessageReceived(object? sender, IMessage e)
{
    Console.WriteLine($"{e.IP} : {e.Payload}");
}
```
###Output

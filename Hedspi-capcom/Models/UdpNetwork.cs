using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Hedspi_capcom.Models
{
	class UdpNetwork
	{
		public void SendBroadcast(string IP, int port, string message)
		{
			IPAddress broadcast = IPAddress.Parse(IP);
			SendBroadcast(broadcast, port, message);
		}

		public void SendBroadcast(UnicastIPAddressInformation IP, int port, string message)
		{
			byte[] ipBytes = IP.Address.GetAddressBytes();
			byte[] maskBute = IP.IPv4Mask.GetAddressBytes();
			for (int i = 0; i < 4; i++)
			{
				ipBytes[i] |= (byte)~maskBute[i];
			}
			IPAddress broadcast = new IPAddress(ipBytes);

			SendBroadcast(broadcast, port, message);
		}

		public void SendBroadcast(IPAddress IP, int port, string message)
		{
			Console.WriteLine("Sending broadcast to {0}:{1}...", IP, port);

			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Blocking = false;

			IPAddress broadcast = IP;

			byte[] sendbuf = Encoding.ASCII.GetBytes(message);
			IPEndPoint ep = new IPEndPoint(broadcast, port);

			s.SendTo(sendbuf, ep);

			s.Close();
		}
	}
}

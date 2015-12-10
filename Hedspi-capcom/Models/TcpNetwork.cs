using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hedspi_capcom.Models
{
	class TcpNetwork
	{
		#region ConnectedHandler property
		private volatile Action<string, NetworkStream> _ConnectedHandler;
		public Action<string, NetworkStream> ConnectedHandler
		{
			get { return _ConnectedHandler; }
			set { _ConnectedHandler = value; }
		}
		#endregion

		#region DisConnectedHandler property
		private volatile Action _DisConnectedHandler;
		public Action DisConnectedHandler
		{
			get { return _DisConnectedHandler; }
			set { _DisConnectedHandler = value; }
		}
		#endregion

		TcpListener Connection = null;

		private bool SocketConnected(Socket s)
		{
			return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
		}

		public void StartListener(int port, CancellationToken ct)
		{
			try
			{
				// TcpListener server = new TcpListener(port);
				Connection = new TcpListener(IPAddress.Any, port);

				// Start listening for client requests.
				Connection.Start();

				// Buffer for reading data
				Byte[] bytes = new Byte[256];
				String data;

				// Enter the listening loop.
				while (true)
				{
					Console.WriteLine("Waiting for a connection... ");

					// Perform a blocking call to accept requests.
					// You could also user server.AcceptSocket() here.
					var client = Connection.AcceptTcpClient();
					Console.WriteLine("Connected!");

					// Get a stream object for reading and writing
					NetworkStream stream = client.GetStream();
					int i;

					// Loop to receive all the data sent by the client.
					while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						// Translate data bytes to a ASCII string.
						data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

						Console.WriteLine("Received: {0}", data);

						if (data.Substring(0, 8) == "HED-Robo")
						{
							while (true)
							{
								if (ct.IsCancellationRequested)
								{
									stream.Close();
									client.Close();
								}
								ct.ThrowIfCancellationRequested();

								var conntectedHandler = ConnectedHandler;
								if (conntectedHandler != null)
								{
									conntectedHandler(data, stream);
								}
							}
						}
						else
						{
							byte[] msg = System.Text.Encoding.ASCII.GetBytes("This is HED-Capcom. You are not Hed-Robo!");
							stream.Write(msg, 0, msg.Length);
							client.Close();
							break;
						}
					}

				}
			}
			catch (SocketException e)
			{
				Console.WriteLine("SocketException: {0}", e);
			}
			catch (OperationCanceledException e)
			{
				var disConntectedHandler = DisConnectedHandler;
				if (disConntectedHandler != null)
				{
					disConntectedHandler();
				}
			}
			finally
			{
				if (Connection != null)
				{
					Connection.Stop();
				}
			}
		}

		public void Stop()
		{
			Connection.Stop();
		}
	}
}

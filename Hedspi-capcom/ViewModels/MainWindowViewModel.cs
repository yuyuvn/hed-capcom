using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hedspi_capcom.Models;
using Hedspi_capcom.Views;
using Livet;
using Livet.Messaging;
using MetroTrilithon.Mvvm;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using XInputDotNetPure;
using System.ComponentModel;
using MetroRadiance;

namespace Hedspi_capcom.ViewModels
{
	public enum CapcomMode
	{
		/// <summary>
		/// Capcom is idle。
		/// </summary>
		Idle,

		/// <summary>
		/// Broadcasting
		/// </summary>
		NotConnected,

		/// <summary>
		/// Connected to robot
		/// </summary>
		Connected,
	}

	/// <summary>
	/// アプリケーションのメイン ウィンドウのためのデータを提供します。このクラスは抽象クラスです。
	/// </summary>
	public class MainWindowViewModel : WindowViewModel
	{
		public IEnumerable<string> Interfaces { get; private set; }

		#region Status 変更通知プロパティ

		private CapcomMode _Status = CapcomMode.Idle;

		public CapcomMode Status
		{
			get { return this._Status; }
			private set
			{
				if (this._Status != value)
				{
					this._Status = value;

					switch (value)
					{
						case CapcomMode.Idle:
							ThemeService.Current.ChangeAccent(Accent.Purple);
							break;
						case CapcomMode.NotConnected:
							ThemeService.Current.ChangeAccent(Accent.Blue);
							break;
						case CapcomMode.Connected:
							ThemeService.Current.ChangeAccent(Accent.Orange);
							break;
					}
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IPList 変更通知プロパティ

		private IEnumerable<string> _IPList;

		public IEnumerable<string> IPList
		{
			get { return this._IPList; }
			private set
			{
				if (this._IPList != value)
				{
					this._IPList = value;
					IPAddress = IPList.SingleOrDefault();
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Interface 変更通知プロパティ

		private NetworkInterface _Interface;

		public string Interface
		{
			get { return this._Interface.Name; }
			set
			{
				if (this._Interface == null || this._Interface.Name != value)
				{
					this._Interface = NetworkInterface.GetAllNetworkInterfaces().First(i => i.Name == value);
					this.RaisePropertyChanged();
					UpdateIPList();
				}
			}
		}

		#endregion

		#region IPAddress 変更通知プロパティ

		private UnicastIPAddressInformation _IPAddress;

		public string IPAddress
		{
			get { return this._IPAddress.Address.ToString(); }
			set
			{
				if (this._IPAddress == null || this._IPAddress.Address.ToString() != value)
				{
					var test = this._Interface.GetIPProperties().UnicastAddresses;
					this._IPAddress = this._Interface.GetIPProperties().UnicastAddresses.First(u => u.Address.ToString() == value);
					this.RaisePropertyChanged();
					this.RaisePropertyChanged("IPAddressInformation");
				}
			}
		}

		#endregion

		#region IPAddressInformation 変更通知プロパティ

		public UnicastIPAddressInformation IPAddressInformation
		{
			get { return this._IPAddress; }
		}

		#endregion

		public string Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}

		private UdpNetwork Broadcast;
		private TcpNetwork Capcom;
		private Video Video;
		private CancellationTokenSource CToken;
		private CancellationTokenSource CToken2;

		private CommandSenderVersionSelector CommandHandler;

		public MainWindowViewModel()
		{
			Title = "Hedspi Capcom";
			Interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback
				&& i.OperationalStatus == OperationalStatus.Up).Select(i => i.Name);
			Interface = Interfaces.First();

			Broadcast = new UdpNetwork();
			Capcom = new TcpNetwork();

			Video = new Video();
			this.CompositeDisposable.Add(Video);

			Capcom.ConnectedHandler += (String messages, NetworkStream stream, TcpClient client) =>
			{
				if (Status != CapcomMode.Connected)
				{
					CommandHandler = new CommandSenderVersionSelector();
					if (CommandHandler.StartSend(messages, stream))
					{
                        Console.WriteLine("Robo connected.");
						Status = CapcomMode.Connected;
						CToken.Cancel();
					}
					else
					{
						Console.WriteLine("Data received: {0}", messages);
                        stream.Close();
						client.Close();
					}
				}
				else
				{
					if (!CommandHandler.Send(stream))
					{
						StartBroadcast();
					}
				}
				
			};

			Capcom.DisConnectedHandler += () =>
			{
				if (Status != CapcomMode.Connected) return;

				Console.WriteLine("Robo disconnected.");
				Status = CapcomMode.Idle;
			};
		}

		private void UpdateIPList()
		{
			UnicastIPAddressInformationCollection UnicastIPInfoCol = _Interface.GetIPProperties().UnicastAddresses;
			IPList = UnicastIPInfoCol.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork).Select(u => u.Address.ToString());
		}

		public void StartBroadcast()
		{
			switch (Status)
			{
				case CapcomMode.Idle:
					if (!Video.Start(12345)) break;
					CToken = new CancellationTokenSource();
					CToken2 = new CancellationTokenSource();
					CancellationToken ct = CToken.Token;
					CancellationToken ct2 = CToken2.Token;
					Status = CapcomMode.NotConnected;
					Task.Factory.StartNew(() => {
						while (!ct.IsCancellationRequested)
						{
							//ct.ThrowIfCancellationRequested();
							Broadcast.SendBroadcast(IPAddressInformation, 11000, String.Format("HED-Capcom v{0}\nIP:{1}\nCapcom:12000\nStream:12345", Version, IPAddressInformation.Address));
							GamePadState state = GamePad.GetState(PlayerIndex.One);
							if (state.IsConnected)
							{
								if (state.Buttons.Start == ButtonState.Pressed)
								{
									StartBroadcast();
								}
							}
							Thread.Sleep(1000);
						}
					}, CToken.Token);

					Task.Factory.StartNew(() => {
						Capcom.StartListener(12000, ct2);
					}, CToken2.Token);
					break;
				case CapcomMode.NotConnected:
					Video.Stop();
					Capcom.Stop();
					CToken.Cancel();
					Status = CapcomMode.Idle;
					break;
				case CapcomMode.Connected:
					CToken2.Cancel();
					Capcom.Stop(); // for some random case
					Video.Stop();
					Status = CapcomMode.Idle;
					break;
			}
		}

		public void Closing()
		{
			Video.Stop();
			Console.WriteLine("Closing");
		}
	}
}

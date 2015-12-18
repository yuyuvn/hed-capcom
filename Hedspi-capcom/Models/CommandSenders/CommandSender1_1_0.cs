using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using XInputDotNetPure;

namespace Hedspi_capcom.Models.CommandSenders
{
	class CommandSender1_1_0 : CommandSender
	{
		private Version version = new Version("1.1.0");

		public override Boolean Send(NetworkStream stream)
		{
			byte[] buffer = null;

			try
			{
				GamePadState state;
                for (int i = 0;i<4;i++)
				{
					switch (i)
					{
						case 1:
							state = GamePad.GetState(PlayerIndex.Two);
							break;
						case 2:
							state = GamePad.GetState(PlayerIndex.Three);
							break;
						case 3:
							state = GamePad.GetState(PlayerIndex.Four);
							break;
						default:
							state = GamePad.GetState(PlayerIndex.One);
							break;
					}
					if (state.IsConnected)
					{
						if (state.Buttons.Start == ButtonState.Pressed)
						{
							return false;
						}
						buffer = new byte[20];

						float leftX = state.ThumbSticks.Left.X;
						float triggerRight = state.Triggers.Right;
						float triggerLeft = state.Triggers.Left;


						if (state.DPad.Left == ButtonState.Pressed || state.DPad.Right == ButtonState.Pressed)
						{
							leftX = state.DPad.Left == ButtonState.Pressed ? -0.5F : 0.5F;
						}
						if (state.DPad.Up == ButtonState.Pressed)
						{
							triggerRight = 0.5F;
						}
						if (state.DPad.Down == ButtonState.Pressed)
						{
							triggerLeft = 0.5F;
						}

						BitConverter.GetBytes(leftX).CopyTo(buffer, 0);
						BitConverter.GetBytes(state.ThumbSticks.Left.Y).CopyTo(buffer, 4);
						BitConverter.GetBytes(triggerRight).CopyTo(buffer, 8);
						BitConverter.GetBytes(triggerLeft).CopyTo(buffer, 12);
						BitConverter.GetBytes(state.Buttons.X == ButtonState.Pressed).CopyTo(buffer, 16);
						BitConverter.GetBytes(state.Buttons.Y == ButtonState.Pressed).CopyTo(buffer, 17);
						BitConverter.GetBytes(state.Buttons.B == ButtonState.Pressed).CopyTo(buffer, 18);
						BitConverter.GetBytes(state.Buttons.A == ButtonState.Pressed).CopyTo(buffer, 19);
						break;
					}
				}
				if (buffer == null)
				{
					buffer = new byte[20];
					Array.Clear(buffer, 0, buffer.Length);
				}
				stream.Write(buffer, 0, buffer.Length);
			}
			catch (System.IO.IOException)
			{
				throw;
			}
			return true;
		}

		public override Version Version()
		{
			return version;
        }

		public override bool StartSend(string message, NetworkStream stream)
		{
			return GetVersionFromMessage(message) >= new Version("1.1");
        }
	}
}

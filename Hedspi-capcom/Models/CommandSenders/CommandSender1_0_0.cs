﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using XInputDotNetPure;

namespace Hedspi_capcom.Models.CommandSenders
{
	class CommandSender1_0_0 : CommandSender
	{
		private Version version = new Version("1.0.0");

		public override Boolean Send(NetworkStream stream)
		{
			byte[] buffer;

			try
			{
				GamePadState state = GamePad.GetState(PlayerIndex.One);
				if (state.IsConnected)
				{
					if (state.Buttons.Start == ButtonState.Pressed)
					{
						return false;
					}
					buffer = new byte[16];

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

					stream.Write(buffer, 0, buffer.Length);
				}
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
			return GetVersionFromMessage(message) >= new Version("1.0");
		}
	}
}

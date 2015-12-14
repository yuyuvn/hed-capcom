using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Hedspi_capcom.Models.CommandSenders;
using System.Reflection;

namespace Hedspi_capcom.Models
{
	public class CommandSenderVersionSelector : ICommandSender
	{
		private ICommandSender commandSender;

		public Boolean Send(NetworkStream stream)
		{
			return commandSender.Send(stream);
		}

		public Version Version()
		{
			throw new NotImplementedException();
		}

		public bool StartSend(string message, NetworkStream stream)
		{
			commandSender = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && typeof(ICommandSender).IsAssignableFrom(t) && t.Namespace == @"Hedspi_capcom.Models.CommandSenders")
				.Select(t => (ICommandSender)Activator.CreateInstance(t))
				.Where(o => o.StartSend(message, stream))
				.OrderByDescending(t => t.Version()).FirstOrDefault();

			if (commandSender == null) return false;
			return true;
		}
	}
}

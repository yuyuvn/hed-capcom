using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedspi_capcom.Models
{
	public abstract class CommandSender : ICommandSender
	{
		public abstract bool StartSend(string message, NetworkStream stream);
		public abstract Boolean Send(NetworkStream stream);
		public abstract Version Version();

		static Regex rgx = new Regex(@"HED-Robo v(?<version>[0-9\\.]+)");

		protected Version GetVersionFromMessage(String message)
		{
			MatchCollection matches = rgx.Matches(message);
			if (matches.Count > 0)
			{
				return new Version(matches[0].Groups["version"].Value);
            } else
			{
				throw new NotSupportedException(String.Format("Data received: {0}", message));
			}
		}
	}
}

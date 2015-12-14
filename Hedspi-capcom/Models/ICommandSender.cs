using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedspi_capcom.Models
{
	public interface ICommandSender
	{
		Boolean StartSend(String message, System.Net.Sockets.NetworkStream stream);
		Boolean Send(System.Net.Sockets.NetworkStream stream);
		Version Version();
	}
}

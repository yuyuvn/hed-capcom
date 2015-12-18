using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hedspi_capcom.Models
{
	class Video : IDisposable
	{
		[DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll")]
		static extern bool CloseWindow(IntPtr hWnd);

		private string FilePath;

		private volatile Process Process;

		public Video()
		{
			FilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ffplay.exe");
		}

		public bool Start(int port)
		{
			Process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.CreateNoWindow = true;
			startInfo.FileName = FilePath;
			startInfo.Arguments = String.Format("-window_title Streaming -autoexit -fflags nobuffer -an rtp://0.0.0.0:{0} -analyzeduration 500", port);
			Process.StartInfo = startInfo;

			try
			{
				Process.Start();
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return OpenFileDialog();
			}
			return true;
        }

		public void Stop()
		{
			if (this.Process != null)
			{
				try
				{
					Close(this.Process.Handle);
					this.Process.Kill();
				}
				catch { }
			}
		}

		public void Dispose()
		{
			Stop();
		}

		void Close(IntPtr hParent)
		{
			IntPtr currChild = IntPtr.Zero;
			currChild = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Streaming");
			if (currChild == IntPtr.Zero) return;
			CloseWindow(currChild);
		}

		private bool OpenFileDialog()
		{
			// Create an instance of the open file dialog box.
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			// Set filter options and filter index.
			openFileDialog1.Filter = "FF player|ffplay.exe";
			openFileDialog1.FilterIndex = 1;

			openFileDialog1.Multiselect = false;

			// Call the ShowDialog method to show the dialog box.
			bool? userClickedOK = openFileDialog1.ShowDialog();

			// Process input if the user clicked OK.
			if (userClickedOK == true)
			{
				Process.StartInfo.FileName = FilePath = openFileDialog1.FileName;
				try
				{
					Process.Start();
				}
				catch (System.ComponentModel.Win32Exception)
				{
					return false;
				}
				return true;
			}
			return false;
		}

	}
}

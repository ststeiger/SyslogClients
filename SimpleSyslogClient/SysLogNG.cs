/* Copyright 2010 Michiel Fortuin
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * */



// http://blog.micfort.org/2011/06/syslog-c.html

// Usage
// Syslog s = new Syslog(new SenderUdp(new IPEndPoint(IPAddress.Parse("192.168.250.2"), 514)));
// s.SendMessage(Level.Emergency, Facility.Local0, "", "Hello world. This is a log message");




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Syslog
{
	/// <summary>
	/// the level of severity
	/// </summary>
	public enum Level
	{
		Emergency = 0,
		Alert = 1,
		Critical = 2,
		Error = 3,
		Warning = 4,
		Notice = 5,
		Information = 6,
		Debug = 7,
	}

	/// <summary>
	/// facility from where a message comes from
	/// </summary>
	public enum Facility
	{
		Kernel = 0,
		User = 1,
		Mail = 2,
		Daemon = 3,
		Auth = 4,
		Syslog = 5,
		Lpr = 6,
		News = 7,
		UUCP = 8,
		Clock = 9,
		Auth2 = 10,
		FTP = 11,
		NTP = 12,
		LogAudit = 13,
		LogAlert = 14,
		Clock2 = 15,
		Local0 = 16,
		Local1 = 17,
		Local2 = 18,
		Local3 = 19,
		Local4 = 20,
		Local5 = 21,
		Local6 = 22,
		Local7 = 23
	}

	public abstract class Sender
	{
		internal abstract void SendMessage(byte[] data);
	}

	public class SenderUdp: Sender
	{
		private UdpClient client;
		public IPEndPoint SyslogServer { get; set; }

		/// <summary>
		/// Creates a sender for syslog class
		/// </summary>
		/// <param name="local">the local endpoint from where the client must send the datagrams</param>
		/// <param name="syslogServer">the server to wich it must send its datagrams</param>
		public SenderUdp(IPEndPoint local, IPEndPoint syslogServer)
		{
			this.client = new UdpClient(local);
			this.SyslogServer = syslogServer;
		}

		/// <summary>
		/// Creates a sender for the syslog class
		/// </summary>
		/// <param name="syslogServer"></param>
		public SenderUdp(IPEndPoint syslogServer)
		{
			this.client = new UdpClient();
			this.SyslogServer = syslogServer;
		}

		~SenderUdp()
		{
			client.Close();
		}

		/// <summary>
		/// Sends a message.
		/// </summary>
		/// <remarks>This method should only be called from the syslog class</remarks>
		/// <param name="data">array with data</param>
		internal override void SendMessage(byte[] data)
		{
			client.Send(data, data.Length, SyslogServer);
		}
	}

	public class Syslog
	{
		private Sender transport;
		private const string NILVALUE = "-";

		/// <summary>
		/// Application name that must be send with the messages
		/// </summary>
		public string AppName { get; set; }
		/// <summary>
		/// The process id that must be send with the messages. if this is not set, null or empty it wil use the PID.
		/// </summary>
		public string ProcID { get; set; }
		/// <summary>
		/// the protocol version
		/// </summary>
		public const int VERSION = 1;

		/// <summary>
		/// Creates a Syslog client
		/// </summary>
		/// <param name="transport">The transport protocol that should be used.</param>
		public Syslog(Sender transport)
		{
			this.transport = transport;
		}

		/// <summary>
		/// constructs a message with a set of parameters
		/// </summary>
		/// <param name="level"></param>
		/// <param name="facility"></param>
		/// <param name="messageID"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private byte[] ConstructMessage(Level level, Facility facility, string messageID, string message = "")
		{
			int prival = ((int)facility) * 8 + ((int)level);
			string pri = string.Format("<{0}>", prival);
			string timestamp =
				new DateTimeOffset(DateTime.Now, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)).ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz");
			string hostname = Dns.GetHostEntry(Environment.UserDomainName).HostName;
			string appName = string.IsNullOrWhiteSpace(this.AppName) ? NILVALUE : this.AppName;
			string procId = string.IsNullOrWhiteSpace(this.ProcID) ? Process.GetCurrentProcess().Id.ToString() : this.ProcID;
			string msgId = string.IsNullOrWhiteSpace(messageID) ? NILVALUE : messageID;

			string header = string.Format("{0}{1} {2} {3} {4} {5} {6}", pri, VERSION, timestamp, hostname, appName, procId, msgId);
			string SD = NILVALUE;

			List<byte> syslogMsg = new List<byte>();
			syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(header));
			syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(" "));
			syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(SD));
			if (message != "")
			{
				syslogMsg.AddRange(System.Text.Encoding.ASCII.GetBytes(" BOM"));
                // The above line should be this instead
                //syslogMsg.AddRange(new byte[] { 0x20, 0xEF, 0xBB, 0xBF }); // Add space plus Unicode byte order mask (BOM) based on the details in RFC 5424 section 6.4.


				syslogMsg.AddRange(System.Text.Encoding.UTF8.GetBytes(message));
			}

			return syslogMsg.ToArray();
		}

		/// <summary>
		/// Sends a message to a syslog server
		/// </summary>
		/// <param name="level">The level of the message</param>
		/// <param name="facility">The facility from where it is send</param>
		/// <param name="messageId">The message id that must be send with the message</param>
		/// <param name="message">The message. This is optional as the protocol describes it.</param>
		public void SendMessage(Level level, Facility facility, string messageId, string message = "")
		{
			transport.SendMessage(ConstructMessage(level, facility, messageId, message));
		}
	}
}
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

// Post-build event command line
// c:\windows\microsoft.net\framework\v2.0.50727\installutil.exe /u "$(TargetPath)"
// c:\windows\microsoft.net\framework\v2.0.50727\installutil.exe "$(TargetPath)"

namespace Syslogd
{
	public class Syslogd
	{
		private bool m_running;
		private Thread m_thread;
		private Socket m_socket;
		private string m_EventLog;
		private Regex m_regex;

		#region Pri
		/// <summary>
		/// Facility  according to http://www.ietf.org/rfc/rfc3164.txt 4.1.1 PRI Part
		/// </summary>
		private enum FacilityEnum : int
		{
			kernel		= 0,	// kernel messages
			user		= 1,	// user-level messages
			mail		= 2,	// mail system
			system		= 3,	// system daemons
			security	= 4,	// security/authorization messages (note 1)
			internally	= 5,	// messages generated internally by syslogd
			printer		= 6,	// line printer subsystem
			news		= 7,	// network news subsystem
			uucp		= 8,	// UUCP subsystem
			cron		= 9,	// clock daemon (note 2) changed to cron
			security2	= 10,	// security/authorization messages (note 1)
			ftp			= 11,	// FTP daemon
			ntp			= 12,	// NTP subsystem
			audit		= 13,	// log audit (note 1)
			alert		= 14,	// log alert (note 1)
			clock2		= 15,	// clock daemon (note 2)
			local0		= 16,	// local use 0  (local0)
			local1		= 17,	// local use 1  (local1)
			local2		= 18,	// local use 2  (local2)
			local3		= 19,	// local use 3  (local3)
			local4		= 20,	// local use 4  (local4)
			local5		= 21,	// local use 5  (local5)
			local6		= 22,	// local use 6  (local6)
			local7		= 23,	// local use 7  (local7)
		}

		/// <summary>
		/// Severity  according to http://www.ietf.org/rfc/rfc3164.txt 4.1.1 PRI Part
		/// </summary>
		private enum SeverityEnum : int
		{
			emergency	= 0,	// Emergency: system is unusable
			alert		= 1,	// Alert: action must be taken immediately
			critical	= 2,	// Critical: critical conditions
			error		= 3,	// Error: error conditions
			warning		= 4,	// Warning: warning conditions
			notice		= 5,	// Notice: normal but significant condition
			info		= 6,	// Informational: informational messages
			debug		= 7,	// Debug: debug-level messages
		}

		private struct Pri
		{
			public FacilityEnum Facility;
			public SeverityEnum Severity;
			public Pri(string strPri)
			{
				int intPri = Convert.ToInt32(strPri);
				int intFacility = intPri >> 3;
				int intSeverity = intPri & 0x7;
				this.Facility = (FacilityEnum)Enum.Parse(typeof(FacilityEnum), intFacility.ToString());
				this.Severity = (SeverityEnum)Enum.Parse(typeof(SeverityEnum), intSeverity.ToString());
			}
			public override string ToString()
			{
				return string.Format("{0}.{1}", this.Facility, this.Severity);
			}
		}
		#endregion

		public Syslogd()
		{
			m_running = false;
			m_thread = null;
			m_socket = null;
			m_EventLog = null;

			/*
			 * The PRI part MUST have three, four, or five characters and will be
			 * bound with angle brackets as the first and last characters.  The PRI
			 * part starts with a leading "<" ('less-than' character), followed by a
			 * number, which is followed by a ">" ('greater-than' character).  
			 */
			m_regex = new Regex("<([0-9]{1,3})>", RegexOptions.Compiled);
		}

		/// <summary>
		/// Starting Syslog worker thread
		/// </summary>
		public void Start()
		{
			Stop();

			m_running = true;
			m_thread = new Thread(new ThreadStart(Worker));
			m_thread.IsBackground = true;
			m_thread.Name = "Worker";
			m_thread.Start();
		}

		/// <summary>
		/// Stopping Syslog worker thread, if running
		/// </summary>
		public void Stop()
		{
			m_running = false;
			if (m_socket != null)
				m_socket.Close(); // Causes SocketException, but thats OK.
			m_socket = null;
			if (m_thread != null)
				m_thread.Join(1000);
			m_thread = null;
		}

		/// <summary>
		/// Evaluator is being used to translate every decimal Pri header in
		/// a Syslog message to an 'Facility.Severity ' string.
		/// </summary>
		/// <param name="match">Any Pri header match in a message</param>
		/// <returns>Translated decimal Pri header to 'Facility.Severity '</returns>
		private string evaluator(Match match)
		{
			Pri pri = new Pri(match.Groups[1].Value);

			return pri.ToString()+" ";
		}

		/// <summary>
		/// Translates Severity type to Syslog type,
		/// a little bit fuzzy because there are less EventLogEntryTypes
		/// than there are syslog Severity levels
		/// </summary>
		/// <param name="Severity">Syslog Severity level</param>
		/// <returns>translated EventLogEntryType</returns>
		private EventLogEntryType Severity2EventLogEntryType(SeverityEnum Severity)
		{
			EventLogEntryType eventLogEntryType;

			switch (Severity)
			{
				case SeverityEnum.emergency:
					eventLogEntryType = EventLogEntryType.Error;
					break;
				case SeverityEnum.alert:
					eventLogEntryType = EventLogEntryType.Error;
					break;
				case SeverityEnum.critical:
					eventLogEntryType = EventLogEntryType.Error;
					break;
				case SeverityEnum.error:
					eventLogEntryType = EventLogEntryType.Error;
					break;
				case SeverityEnum.warning:
					eventLogEntryType = EventLogEntryType.Warning;
					break;
				case SeverityEnum.notice:
					eventLogEntryType = EventLogEntryType.Information;
					break;
				case SeverityEnum.info:
					eventLogEntryType = EventLogEntryType.Information;
					break;
				case SeverityEnum.debug:
					eventLogEntryType = EventLogEntryType.Information;
					break;
				default: // ?
					eventLogEntryType = EventLogEntryType.Error;
					break;
			}
			return eventLogEntryType;
		}

		/// <summary>
		/// Translates Syslog messages to Eventlog messages
		/// Using Pri part as source, and log them to Windows EventLog
		/// </summary>
		/// <param name="endPoint">IP/port number from datagram sender</param>
		/// <param name="strReceived">Syslog message</param>
		private void Log(EndPoint endPoint, string strReceived)
		{
			Pri pri = new Pri(m_regex.Match(strReceived).Groups[1].Value);

			EventLogEntryType eventLogEntryType = Severity2EventLogEntryType(pri.Severity);

			string strMessage = string.Format("{0} : {1}", endPoint, m_regex.Replace(strReceived, evaluator));

			EventLog myLog = new EventLog(m_EventLog);
			myLog.Source = pri.ToString();
			myLog.WriteEntry(strMessage, eventLogEntryType);
			myLog.Close();
			myLog.Dispose();
		}

		/// <summary>
		/// Worker thread, setting up an listening socket for UDP datagrams
		/// </summary>
		private void Worker()
		{
			Properties.Settings settings = new Properties.Settings();

			m_EventLog = settings.EventLog;

			IPAddress ipAddress = IPAddress.Any;
			if(settings.Address!="*")
				ipAddress = IPAddress.Parse(settings.Address);

			IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, settings.Port);

			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_socket.Bind(ipEndPoint);


			// Recycling vars , i love it.
			EndPoint endPoint = ipEndPoint;

			// http://www.ietf.org/rfc/rfc3164.txt
			// 4.1 syslog Message Parts
			// The total length of the packet MUST be 1024 bytes or less.
			byte[] buffer = new byte[1024];

			while (m_running)
			{
				try
				{
					int intReceived = m_socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
					string strReceived = Encoding.ASCII.GetString(buffer, 0, intReceived);
					Log(endPoint, strReceived);
				}
				catch (Exception exception)
				{
					EventLog myLog = new EventLog();
					myLog.Source = settings.ServiceName;
					if (!m_running)
						myLog.WriteEntry("Stopping...", EventLogEntryType.Information);
					else
						myLog.WriteEntry(exception.Message, EventLogEntryType.Error);
					myLog.Close();
					myLog.Dispose();
				}
			}
		}
	}
}

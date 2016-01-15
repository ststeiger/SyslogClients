
// http://stackoverflow.com/questions/20951667/how-to-write-to-kiwi-syslog-server-log-c-sharp

// SyslogNet
// .Net Syslog client. Supports both RFC 3164 and RFC 5424 Syslog standards as well as UDP and encrypted TCP transports.
// https://github.com/emertechie/SyslogNet
// https://www.nuget.org/packages/SyslogNet.Client/0.3.0
// https://www.nuget.org/api/v2/package/SyslogNet.Client/0.3.0


// using SyslogNet;
// using SyslogNet.Client;
// using SyslogNet.Client.Serialization;
// using SyslogNet.Client.Transport;


namespace AdvancedSyslogClient
{

    // Next:
    // https://www.nuget.org/packages/Portable.Syslog/0.4.0-alpha

    static class Program
    {


        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
            if (false)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new Form1());
            }

            
            var _syslogSender = new SyslogNet.Client.Transport.SyslogEncryptedTcpSender("localhost", 514, -1, true);
            //var _syslogSender = new SyslogNet.Client.Transport.SyslogTcpSender("localhost", 514);
            // var _syslogSender = new SyslogNet.Client.Transport.SyslogUdpSender("localhost", 514);
            _syslogSender.Send(
                 new SyslogNet.Client.SyslogMessage(
                    System.DateTime.Now,
                    SyslogNet.Client.Facility.SecurityOrAuthorizationMessages1,
                    SyslogNet.Client.Severity.Informational,
                    System.Environment.MachineName,
                    "Application Name",
                    "Message Content"
                    )
                , new SyslogNet.Client.Serialization.SyslogRfc3164MessageSerializer()
            );

        } // End Sub Main


    } // End Class Program


} // End Namespace AdvancedSyslogClient

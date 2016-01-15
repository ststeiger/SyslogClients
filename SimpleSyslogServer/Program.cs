
namespace SimpleSyslogServer
{

    // https://evilzone.org/vb-vb-net-c-c-net/%28c%29-simple-udp-server-syslog-example/
    static class Program
    {

        private static System.Threading.ManualResetEvent _exiting = new System.Threading.ManualResetEvent(false);

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [System.STAThread]
        static void Main(string[] args)
        {
            if (false)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new Form1());
            }

            System.Console.CancelKeyPress += (sender, eventArgs) => _exiting.Set();
            UdpListener(514);
            _exiting.WaitOne();
        } // End Sub Main 


        private static void UdpListener(int port)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                using (var udpClient = new System.Net.Sockets.UdpClient(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port)))
                {
                    while (true)
                    {
                        var receivedResults = await udpClient.ReceiveAsync();
                        var s = string.Format("[{0}] {1}", receivedResults.RemoteEndPoint, System.Text.Encoding.ASCII.GetString(receivedResults.Buffer));
                        System.Console.WriteLine(s);
                    }
                } // End Using

            }); // End Task.Run

        } // End Sub UdpListener


    } // End Class Program 


} // End Namespace SimpleSyslogServer 

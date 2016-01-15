
// using System.IO;
// using System.Text;
// using System.Net;
// using System.Net.Sockets;
// using System.Threading;
// using System.Net.Mail;


// http://www.codeproject.com/Tips/441233/Multithreaded-Customizable-SysLog-Server-Csharp
namespace MultithreadedSyslogServer
{


    class Test
    {

        static void RunServer()
        {
            System.Net.IPEndPoint anyIP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
            System.Net.Sockets.UdpClient udpListener = new System.Net.Sockets.UdpClient(514);
            byte[] bReceive; string sReceive; string sourceIP;

            /* Main Loop */
            /* Listen for incoming data on udp port 514 (default for SysLog events) */
            while (true)
            {
                try
                {
                    bReceive = udpListener.Receive(ref anyIP);
                    
                    // Convert incoming data from bytes to ASCII
                    sReceive = System.Text.Encoding.ASCII.GetString(bReceive);
                    
                    // Get the IP of the device sending the syslog
                    sourceIP = anyIP.Address.ToString();

                    // Start a new thread to handle received syslog event
                    new System.Threading.Thread(new logHandler(sourceIP, sReceive).handleLog).Start();
                    
                }
                catch (System.Exception ex) 
                { 
                    System.Console.WriteLine(ex.ToString()); 
                }
            } // Whend

        } // End Sub Main 


    } // End Class Test


}

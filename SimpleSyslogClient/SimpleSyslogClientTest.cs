
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SimpleSyslogClient.Tests
{


    public class TestClient
    {
        public static void RunClient()
        {
            Client c = new Client();
            try
            {
                //c.Port= 1200;
                c.SysLogServerIp = "127.0.0.1";  // syslogd on local machine
                int facility = (int)Facility.User; // Local5
                int level = (int)Level.Warning;  // Debug;
                string text = "Hello, Syslog World.";

                c.Send(new Message(facility, level, text));
            }
            catch (System.Exception ex1)
            {
                Console.WriteLine("Exception! " + ex1);
            }
            finally
            {
                c.Close();
            }
        }
    }


}

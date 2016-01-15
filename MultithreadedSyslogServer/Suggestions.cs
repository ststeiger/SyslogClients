
// http://www.codeproject.com/Tips/441233/Multithreaded-Customizable-SysLog-Server-Csharp
// http://www.codeproject.com/Articles/18086/Syslog-daemon-for-Windows-Eventlog
// https://x443.wordpress.com/2012/04/03/c-net-syslog-server-with-sql/
// http://www.fantail.net.nz/wordpress/?p=5

// http://serverfault.com/questions/6110/good-syslog-server-for-windows
// https://github.com/ststeiger/aonawaresyslog/
// https://code.google.com/p/aonawaresyslog/


// https://github.com/mcuadros/go-syslog


namespace MultithreadedSyslogServer
{

    /*
     You are creating a two new threads for every message that you receive (one to handle the message with logHandler.handleLog, 
     * and another created in that function that executes outputCsvRow). 
     * Creating threads is a pretty expensive process and you should avoid creating them in this way.

     Instead, you should create at most one additional thread that handles a Queue, and trigger that thread with an AutoResetEvent object 
      (there are many other equivalent ways also). That is, in your while(true) loop capture the data, push it into a Queue 
      (make sure you lock() it first) then call the trigger. 
     That allows your receiving code to act very quickly by simply receiving the data and passing it on 
     to another thread to do something with the data. Something like this:
     
     In this case, what you are doing (appending to a CSV file) is so simple that having a separate thread to write to the CSV file is overkill. 
     However, you may want to have another thread to handle sending the email, since that could take some time depending on your environment. 
     In that case, create a second queue and a second thread, and handle it in exactly the same way. 
     That way, you don't block writing to the CSV file because your email environment is slow.
     */

    class Suggestions
    {

        private static System.Collections.Generic.Queue<string> Messages = new System.Collections.Generic.Queue<string>();
        private static System.Threading.AutoResetEvent MessageTrigger = new System.Threading.AutoResetEvent(false);


        static void MainEntryPoint()
        {
            // Start processing thread
            System.Threading.Thread handler = new System.Threading.Thread(new System.Threading.ThreadStart(HandleMessage));
            handler.IsBackground = true;
            handler.Start();
            while (true)
            {
                // Receive the message
                //byte[] bReceive = udpListener.Receive(ref anyIP);

                // push the message to the queue, and trigger the queue
                // lock(Message) 
                //{ 
                //    Messages.Enqueue(Encoding.ASCII.GetString(bReceive) ); 
                //}
                MessageTrigger.Set();
            } // Whend

        } // End Sub Main 


        private static void HandleMessage()
        {
            while (true)
            {
                MessageTrigger.WaitOne(5000);    // A 5000ms timeout to force processing
                string[] messages = null;
                lock (Messages)
                {
                    messages = Messages.ToArray();
                    Messages.Clear();
                }

                foreach (string msg in messages)
                {
                    // Write message to file...
                    // WriteToCSV(msg);

                    // Test msg for conditions and send email if needed
                    // if (...) { SendEmail(msg); }
                }
            } // Whend

        } // End Sub HandleMessage 


    } // End Class Suggestions


} // End Namespace MultithreadedSyslogServer

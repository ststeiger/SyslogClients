
namespace MultithreadedSyslogServer
{


    class logHandler
    {
        // Phrases within the syslog that will trigger an email notification
        private string[] emailTriggers = new string[] { "link loss", "help please" };
        private string outputPath = @"C:\Users\metastruct\Desktop\syslog.csv"; // Location to store events
        private string source; private string log;


        public logHandler(string sourceIP, string logData) // Initialize object and clean up the raw data
        {
            source = sourceIP.Trim(); // Client IP
            log = logData.Replace(System.Environment.NewLine, "").Trim(); // Syslog data
        } // End Constructor


        public void handleLog() // Store the syslog and determine whether to trigger an email notification
        {
            // Store the syslog using a new thread
            new System.Threading.Thread(new outputCsvRow(outputPath, new string[] { source, log }).addRow).Start();

            // Search for trigger strings and send email if found
            for (int i = 0; i < emailTriggers.Length; i++)
            {
                if (log.Contains(emailTriggers[i])) { emailEvent(); }
            } // Next i 

        } // End Sub handleLog 


        private void emailEvent() // Send email notification
        {
            try
            {
                System.Net.Mail.MailMessage notificationEmail = new System.Net.Mail.MailMessage();
                notificationEmail.Subject = "SysLog Event";
                notificationEmail.IsBodyHtml = true;

                notificationEmail.Body = "<b>SysLog Event Triggered:<br/><br/>Time: </b><br/>" +
                    System.DateTime.Now.ToString() + "<br/><b>Source IP: </b><br/>" +
                    source + "<br/><b>Event: </b><br/>" + log; // Throw in some basic HTML for readability

                notificationEmail.From = new System.Net.Mail.MailAddress("SysLog@metastruct.com", "SysLog Server"); // From Address
                notificationEmail.To.Add(new System.Net.Mail.MailAddress("metastructblog@gmail.com", "metastruct")); // To Address
                System.Net.Mail.SmtpClient emailClient = new System.Net.Mail.SmtpClient("10.10.10.10"); // Address of your SMTP server of choice

                // emailClient.UseDefaultCredentials = false; // If your SMTP server requires credentials to send email
                // emailClient.Credentials = new NetworkCredential("username", "password"); // Supply User Name and Password

                emailClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                emailClient.Send(notificationEmail); // Send the email
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

        } // End Sub emailEvent 


    } // End Class logHandler 


}

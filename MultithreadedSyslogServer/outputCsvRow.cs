
namespace MultithreadedSyslogServer
{


    class outputCsvRow
    {
        private string formattedRow = null;
        private string outputPath = null;

        public outputCsvRow(string filePath, string[] columns) /* Initialize object */
        {
            outputPath = filePath;
            formattedRow = (char)34 + System.DateTime.Now.ToString() + (char)34; /* Construct csv row starting with the timestamp */
            for (int i = 0; i < columns.Length; i++) { formattedRow += "," + (char)34 + columns[i] + (char)34; }
        }

        public void addRow()
        {
            int attempts = 0;
            bool canAccess = false;
            System.IO.StreamWriter logWriter = null;

            if (!System.IO.File.Exists(outputPath)) /* If the file doesn't exist, give it some column headers */
            {
                logWriter = new System.IO.StreamWriter(outputPath, true);
                logWriter.WriteLine((char)34 + "Event_Time" + (char)34 + "," +
                  (char)34 + "Device_IP" + (char)34 + "," + (char)34 + "SysLog" + (char)34);
                logWriter.Close();
            }

            //  Thread safety first! This is a poor man's SpinLock
            while (true)
            {
                try
                {
                    logWriter = new System.IO.StreamWriter(outputPath, true); // Try to open the file for writing
                    canAccess = true; /* Success! */
                    break;
                }
                catch (System.IO.IOException ex)
                {
                    if (attempts < 15) { attempts++; System.Threading.Thread.Sleep(50); }
                    else { System.Console.WriteLine(ex.ToString()); break; } // Give up after 15 attempts
                }
            } // Whend

            if (canAccess) // Write the line if the file is accessible
            {
                logWriter.WriteLine(formattedRow);
                logWriter.Close();
            }

        } // End Sub addRow 


    } // End Class outputCsvRow


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Threading.Tasks;

namespace Utils
{
    public class Logger
    {
        public void W(string text, string filePath = "RegalSimphonyExporter.log")
        {
            text = DateTime.Now + " | " + text;

            if (!File.Exists(filePath))
            {
                var file = File.Create(filePath);
                file.Close();

                var tw = new StreamWriter(filePath, true);

                tw.WriteLine(text);
                tw.Close();
            }
            else if(File.Exists(filePath))
            {
                using (var tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine(text);
                    tw.Close();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models
{
    public class ConsoleProcessor : TextWriter
    {
        private StringWriter ConsoleContent = new StringWriter();
        private FileStream fs = null;
        
        private string FilePath = "";
        public override Encoding Encoding
        {
            get { return null; }
        }

        public ConsoleProcessor(string path)
        {
            FilePath = path;
            fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write);
            fs.Close();
        }

        public string Get()
        {
            return ConsoleContent.ToString();
        }

        public override void WriteLine(string value)
        {
            lock(ConsoleContent)
            {
                Debug.WriteLine(value);
                ConsoleContent.WriteLine(value);

                StreamWriter sw = new StreamWriter(FilePath, true, Encoding.ASCII);
                sw.WriteLine(value);
                sw.Close();
            }

        }
        
    }
}

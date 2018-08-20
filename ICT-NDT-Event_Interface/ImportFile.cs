using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace ICT_NDT_Event_Interface
{
    public class ImportFile
    {
        private string[] _strLines;

        public ImportFile(string path)
        {
            _strLines = File.ReadAllLines(path);
        }

        private string get_value_from_key(string value)
        {
            string strRet = "";
            foreach (var line in _strLines)
            {
                if (line.Contains(value)
                    && line.Contains("="))
                {
                    strRet = line.Split('=')[1].Trim();
                }
            }
            return strRet;
        }

        public object read_parameters()
        {
            return new[]
            {
                get_value_from_key("DeviceID"),
                get_value_from_key("WorkerID")
            };
        }

        public StringBuilder read_log_data()
        {
            using (TextFieldParser parser = new TextFieldParser(get_value_from_key("Path")))
            {
                StringBuilder sb = new StringBuilder();
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    sb.Append(parser.ReadLine()).Append("\r\n");
                }
                return sb;
            }
        }
    }
}
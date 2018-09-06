using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace ICT_NDT_Event_Interface
{
    public class FileIO
    {
        private string[] _strLines;
        private string _path;

        public FileIO(string path)
        {
            _path = path;
            init();
        }

        private void init()
        {
            if (!File.Exists(_path))
            {
                File.WriteAllBytes(_path,new byte[0]);
            }
            _strLines = File.ReadAllLines(_path, Encoding.Default);
        }

        private List<string> get_value_from_key(string value)
        {
            List<string> strRet = new List<string>();
            foreach (var line in _strLines)
            {
                if (line.Contains("="))
                {
                    if (line.Split('=')[0].Trim().Contains(value))
                    {
                        if (line.Split('=').Length > 1)
                        {
                            strRet.Add(line.Split('=')[1].Trim());
                        }
                        else
                        {
                            strRet.Add("");
                        }
                    }
                }
            }
            return strRet;
        }

        public string read_key_from_file(string key)
        {
            return get_value_from_key(key)[0];
        }

        public string[] read_keys_from_file(string key)
        {
            return get_value_from_key(key).ToArray();
        }

        public StringBuilder read_log_data(string path)
        {
            using (TextFieldParser parser = new TextFieldParser(path, Encoding.Default))
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

        public void clear()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
            using (FileStream fs = new FileStream(_path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(new byte[1], 0, 0);
                fs.Close();
                fs.Dispose();
            }
            init();
        }

        public void write_kvp_to_file(string key, string value)
        {
            string strNewKVP = key + " = " + value.Trim('\n', ' ');
            bool hasModified = false;
            for (int i = 0; i < _strLines.Length; i++)
            {
                if (_strLines[i].Contains("="))
                {
                    if (_strLines[i].Split('=').Length > 1)
                    {
                        if (_strLines[i].Split('=')[0].Contains(key))
                        {
                            _strLines[i] = strNewKVP;
                            hasModified = true;
                            break;
                        }
                    }
                }
            }

            if (!hasModified)
            {
                _strLines =
                    new Func<string[], string, string[]>(
                        (x, y) =>
                        {
                            List<string> list = x.ToList();
                            list.Add(y);
                            return list.ToArray();
                        })(_strLines, strNewKVP);
            }
            Console.WriteLine(strNewKVP);
            File.WriteAllLines(
                _path,
                _strLines,
                Encoding.Default
            );
        }
    }
}
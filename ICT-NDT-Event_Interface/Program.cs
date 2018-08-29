using System;
using System.Data;
using System.Text;

namespace ICT_NDT_Event_Interface
{
    class Program
    {
        static void Main(string[] args)
        {
            string path =  args[0];
            ImportFile file = new ImportFile(path);
            object parameters = file.read_parameters();
            StringBuilder sb = file.read_log_data();
            DataTable dt = ParseData.parse_data_into_datatable(sb, parameters);
            SqlServerHelper.SQLConn(dt);
            Console.ReadKey();
        }
    }
}

using System.Data;
using System.Text;

namespace ICT_NDT_Event_Interface
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"E:\Programmes\GitHub\CSharp\ICT-NDT-Event_Interface\ICT-NDT-Event_Interface\source\EventStart.txt"; // args[0];
            ImportFile file = new ImportFile(path);
            object parameters = file.read_parameters();
            StringBuilder sb = file.read_log_data();
            DataTable dt = ParseData.parse_data_into_datatable(sb, parameters);
            //            SqlServerHelper.SQLConn();
            SqlServerHelper.SQLConn(dt);
//            SqlServerHelper.DataTableToSQLServer(dt);
            //Console.ReadKey();
        }
    }
}

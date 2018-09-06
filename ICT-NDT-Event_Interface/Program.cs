using System;
using System.Data;
using System.IO;
using System.Text;

namespace ICT_NDT_Event_Interface
{
    class Program
    {
        static void Main(string[] args)
        {
            string event_path = args[0];
            FileIO fileR = new FileIO(event_path);
            string event_ID = fileR.read_key_from_file("Event");
            string strRetPath = fileR.read_key_from_file("Result");
            FileIO fileW = new FileIO(strRetPath);
            switch (event_ID)
            {
                case "TestStart":
                    try
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "0");
                        fileW.write_kvp_to_file("InfoText", "");
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }
                    break;
                case "TestDone":
                    try
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "0");
                        fileW.write_kvp_to_file("InfoText", "");
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }
                    break;
                case "TestResult":
                    string server, database, uid, password, table;
                    try
                    {
                        INIIO ini = new INIIO(AppDomain.CurrentDomain.SetupInformation.ApplicationBase +
                                              "OffsetTest.ini");
                        server = ini.IniReadValue("Database", "DatabaseServer");
                        database = ini.IniReadValue("Database", "DatabaseName");
                        uid = ini.IniReadValue("Database", "DatabaseUserID");
                        password = ini.IniReadValue("Database", "DatabasePassword");
                        table = ini.IniReadValue("Table", "TableName");
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }

                    SqlServerHelper connection;
                    try
                    {
                        connection = new SqlServerHelper(server, database, uid, password, table);
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }

                    string[] paths;
                    object o;
                    try
                    {
                        paths = fileR.read_keys_from_file("Detail");
                        o = new[]
                        {
                            fileR.read_key_from_file("DeviceID"),
                            fileR.read_key_from_file("WorkerID")
                        };
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }

                    try
                    {
                        foreach (var path in paths)
                        {
                            Console.WriteLine(path);
                            StringBuilder sb = fileR.read_log_data(path);
                            DataTable dt = ParseData.parse_data_into_datatable(
                                sb, o);
                            ParseData.ClearState();
                            connection.dataToServer(dt);
                        }
                    }
                    catch (Exception e)
                    {
                        fileW.clear();
                        fileW.write_kvp_to_file("TestCancel", "1");
                        fileW.write_kvp_to_file("InfoText", e.ToString().Split('\n')[0]);
                        return;
                    }

                    connection.dispose();
                    fileW.clear();
                    fileW.write_kvp_to_file("TestCancel", "0");
                    fileW.write_kvp_to_file("InfoText", "");
                    break;
            }
        }
    }
}
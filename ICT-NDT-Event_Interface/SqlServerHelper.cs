﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ICT_NDT_Event_Interface
{
    public class SqlServerHelper
    {

        public static void SQLConn(DataTable dt)
        {
            SqlConnection Conn = new SqlConnection("Server=192.168.0.248;DataBase=NDTOffset;uid=OffsetTestUser;pwd=ndt@123");

            Conn.Open();

            List<string> str_column_name = new List<string>();
            foreach (DataColumn dc in dt.Columns)
            {
                str_column_name.Add(dc.ColumnName);
            }
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                List<string> str_column_value = new List<string>();
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO Test_2 (");
                sb.Append(string.Join(",", str_column_name));
                sb.Append(") VALUES (");
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    if (dt.Rows[r][c] is float)
                    {
                        str_column_value.Add(dt.Rows[r][c].ToString());
                    }
                    else
                    {
                        str_column_value.Add("\'" + dt.Rows[r][c] + "\'");
                    }
                }
                sb.Append(string.Join(",", str_column_value));
                sb.Append(")");
//                Console.WriteLine(sb.ToString());

                SqlCommand cmd = new SqlCommand(sb.ToString(), Conn);

                DataSet ds = new DataSet();

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                da.Fill(ds);
            }

            Conn.Close();
        }

        public static void DataTableToSQLServer(DataTable dt)
        {
            string connectionString = @"Persist Security Info=False;Initial Catalog=NDTOffset;Data Source=192.168.0.248; User ID=OffsetTestUser; Password=ndt@123";
            using (SqlConnection destinationConnection = new SqlConnection(connectionString))
            {
                destinationConnection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    try
                    {
                        bulkCopy.DestinationTableName = "Test_2";//要插入的表的表名
                        bulkCopy.BatchSize = dt.Rows.Count;
                        foreach (DataColumn dc in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dc.ColumnName, dc.ColumnName));
                        }
                        bulkCopy.WriteToServer(dt);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
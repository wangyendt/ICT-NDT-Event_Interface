using System.Data;
using System.IO;

namespace ICT_NDT_Event_Interface
{
    public class CSVHelper
    {
        /// <summary>
        /// 将CSV文件中内容读取到DataTable中
        /// </summary>
        /// <param name="path">CSV文件路径</param>
        /// <param name="hasTitle">是否将CSV文件的第一行读取为DataTable的列名</param>
        /// <returns></returns>
        public static DataTable ReadFromCSV(string path, bool hasTitle = false)
        {
            DataTable dt = new DataTable();           //要输出的数据表
            StreamReader sr = new StreamReader(path); //文件读入流
            bool bFirst = true;                       //指示是否第一次读取数据

            //逐行读取
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] elements = line.Split(',');

                //第一次读取数据时，要创建数据列
                if (bFirst)
                {
                    for (int i = 0; i < elements.Length; i++)
                    {
                        dt.Columns.Add();
                    }
                    bFirst = false;
                }

                //有标题行时，第一行当做标题行处理
                if (hasTitle)
                {
                    for (int i = 0; i < dt.Columns.Count && i < elements.Length; i++)
                    {
                        dt.Columns[i].ColumnName = elements[i];
                    }
                    hasTitle = false;
                }
                else //读取一行数据
                {
                    if (elements.Length == dt.Columns.Count)
                    {
                        dt.Rows.Add(elements);
                    }
                    else
                    {
                        //throw new Exception("CSV格式错误：表格各行列数不一致");
                    }
                }
            }
            sr.Close();

            return dt;
        }

        /// <summary>
        /// 将DataTable内容保存到CSV文件中
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="path">CSV文件地址</param>
        /// <param name="hasTitle">是否要输出数据表各列列名作为CSV文件第一行</param>
        public static void SaveToCSV(DataTable dt, string path, bool hasTitle = false)
        {
            StreamWriter sw = new StreamWriter(path);

            //输出标题行（如果有）
            if (hasTitle)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write(dt.Columns[i].ColumnName);
                    if (i != dt.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.WriteLine();
            }

            //输出文件内容
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    sw.Write(dt.Rows[i][j].ToString());
                    if (j != dt.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.WriteLine();
            }

            sw.Close();
        }
    }
}
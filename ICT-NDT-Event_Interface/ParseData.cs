using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ICT_NDT_Event_Interface
{
    public class ParseData
    {
        private static List<string> _head_features;    // 仅出现一次的特征名
        private static List<string> _cyc_head_features;    // 循环出现的特征名
        private static List<string> _datatable_features;   // 标记数据起始和终止的特征名
        private static int _step;  // 指示当前寻找的特征名的类别(步骤)
        // 第一类: 头信息
        // 第二类: 循环头信息
        // 第三类: 数据表
        private static int _max_step = 3;   // 最大类别数

        private static List<object>[] _contents;

        private enum DataColumn
        {
            BX = 1,
            Device_Name = 4,
            TestVal = 19,
            Result = 21,
            TestMode = 9,
        }

        private static void create_features()
        {
            _head_features = new List<string>();
            _head_features.Add("DateTime");
            _head_features.Add("BoardName");
            _head_features.Add("BarCode");
            _cyc_head_features = new List<string>();
            _cyc_head_features.Add("OpenTest");
            _cyc_head_features.Add("ShortTest");
            _datatable_features = new List<string>();
            _datatable_features.Add("STEP");
            _datatable_features.Add("LogEnd");
        }

        private static void create_content()
        {
            _contents = new List<object>[_max_step];
            for (int i = 0; i < _contents.Length; i++)
            {
                _contents[i] = new List<object>();
            }
        }

        public static void ClearState()
        {
            _step = 0;
        }

        public static DataTable parse_data_into_datatable(StringBuilder sb, object parameters)
        {
            DataTable dt = CreateTable.create_data_table();
            create_features();
            create_content();
            int cyc_fea_num = 0;
            string strDeviceID = ((string[])parameters)[0];
            string strWorkderID = ((string[])parameters)[1];

            string[] dataCurrent = null;
            string[] dataNext = null;

            int cyc_start_ind = 0;  // 循环特征的索引
            bool bVoltagePass = true;
            bool bResistancePass = true;
            int batch_modify_index_start = 0;   // 批量修改的索引开始处
            int data_offset = 0;   // 数据块首次出现的位置相对于整个文件的偏移(行数)

            string[] strInfos = sb.ToString().Replace("\"","").Split('\n');
            for (int i = 0; i < strInfos.Length; i++)
            {
                //Console.WriteLine(_step + " " + strInfos[i]);
                if (strInfos[i].Contains(_datatable_features[1]))
                {
                    // 如果当前行含有结束符LogEnd, 则停止解析
                    break;
                }
                switch (_step)
                {
                    case 0:
                        foreach (var fea in _head_features)
                        {
                            if (strInfos[i].Contains(fea))
                            {
                                string str_ = strInfos[i].Split(',')[1];
                                if (fea.Contains("DateTime"))
                                {
                                    str_ = Convert.ToDateTime(str_).ToString("yyyyMMdd hh:mm:ss");
                                }
                                _contents[_step].Add(str_);
                                _head_features.RemoveAt(0);
                                if (_head_features.Count == 0)
                                {
                                    _step++;
                                }
                                break;
                            }
                        }
                        break;
                    case 1:
                        // 不同的循环特征, 采用循环的策略添加到list中
                        // _contents[1]中包含M * n个数, M为板子个数(如B1-B8, 共8个)
                        // n为特征数, 如openTest, shortTest, 共2个
                        if (strInfos[i].Contains(_cyc_head_features[cyc_fea_num]))
                        {
                            if (cyc_fea_num == 0)
                            {
                                _contents[_step].Add(new List<object>());
                            }
                            ((List<object>)_contents[_step][_contents[_step].Count - 1]).Add(strInfos[i].Split(',')[1]);
                            cyc_fea_num = (cyc_fea_num + 1) % _cyc_head_features.Count;
                        }
                        if (strInfos[i + 1].Contains(_datatable_features[0]))
                        {
                            _step++;
                        }
                        break;
                    case 2:
                        if (strInfos[i].Contains(_datatable_features[0]))
                        {
                            data_offset = i + 1;
                            break;
                        }
                        dataCurrent = dataNext ?? strInfos[i].Split(',');
                        dataNext = strInfos[i + 1].Split(',');

                        bool bBXchange = dataCurrent[(int)DataColumn.BX] != dataNext[(int)DataColumn.BX];
                        dt.Rows.Add(
                            dataCurrent[(int)DataColumn.BX],
                            dataCurrent[(int)DataColumn.Device_Name],
                            dataCurrent[(int)DataColumn.TestVal],
                            dataCurrent[(int)DataColumn.Result].Trim(),
                            _contents[0][0],
                            _contents[0][1],
                            _contents[0][2],
                            ((List<object>)_contents[1][cyc_start_ind])[0],
                            ((List<object>)_contents[1][cyc_start_ind])[1],
                            null,
                            null,
                            strDeviceID,
                            strWorkderID,
                            dataCurrent[(int)DataColumn.TestMode] == "CV" ? "R" : "V"
                            );
                        if (bBXchange)
                        {
                            cyc_start_ind = (cyc_start_ind + 1) % _contents[1].Count;
                            for (int j = batch_modify_index_start; j < i + 1 - data_offset; j++)
                            {
                                dt.Rows[j][9] = bVoltagePass ? "Pass" : "NG";
                                dt.Rows[j][10] = bResistancePass ? "Pass" : "NG";
                            }
                            batch_modify_index_start = i + 1 - data_offset;
                            bVoltagePass = true;
                            bResistancePass = true;
                        }
                        else
                        {
                            if (dataCurrent[(int)DataColumn.TestMode].Trim() == "CV")
                            {
                                bResistancePass = bResistancePass && dataCurrent[(int)DataColumn.Result].Trim() == "Pass";
                            }
                            else
                            {
                                bVoltagePass = bVoltagePass && dataCurrent[(int)DataColumn.Result].Trim() == "Pass";
                            }
                        }
                        break;
                }
            }
            
//            CSVHelper.SaveToCSV(dt, @"C:\Users\lenovo\Desktop\test.csv");

            return dt;
        }
    }
}
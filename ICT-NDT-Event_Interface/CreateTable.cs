using System.Data;

namespace ICT_NDT_Event_Interface
{
    public static class CreateTable
    {
        public static DataTable create_data_table()
        {
            using (DataTable dataTable = new DataTable())
            {
                dataTable.Columns.Add("N_BX", typeof(string));
                dataTable.Columns.Add("N_DeviceName", typeof(string));
                dataTable.Columns.Add("N_TestVal", typeof(float));
                dataTable.Columns.Add("N_Result", typeof(string));
                dataTable.Columns.Add("N_DateTime", typeof(string));
                dataTable.Columns.Add("N_BoardName", typeof(string));
                dataTable.Columns.Add("N_BarCode", typeof(string));
                dataTable.Columns.Add("N_LotNo", typeof(string));
                dataTable.Columns.Add("N_OpenTest", typeof(string));
                dataTable.Columns.Add("N_ShortTest", typeof(string));
                dataTable.Columns.Add("N_VoltageResult", typeof(string));
                dataTable.Columns.Add("N_ResistanceResult", typeof(string));
                dataTable.Columns.Add("N_MachineID", typeof(string));
                dataTable.Columns.Add("N_Operator", typeof(string));
                dataTable.Columns.Add("N_TestType", typeof(string));
                return dataTable;
            }
        }
    }
}
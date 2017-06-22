using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Common
{
    public class CHelp
    {
        public static DataTable CopyDataTable(DataTable dataTable)
        {
            DataTable dt = new DataTable();
            foreach (DataColumn col in dataTable.Columns)
            {
                DataColumn dtcol = new DataColumn(col.ColumnName);
                dt.Columns.Add(dtcol);
            }
            foreach (DataRow row in dataTable.Rows)
            {
                DataRow resultrow = dt.NewRow();
                foreach (DataColumn col in dataTable.Columns)
                {
                    resultrow[col.ColumnName] = row[col.ColumnName];
                }
                dt.Rows.Add(resultrow);
            }
            return dt;
        }
    }
}

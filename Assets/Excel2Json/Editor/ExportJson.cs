using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using LitJson;
namespace Excel2Json
{
    /// <summary>
    /// 将DataTable对象，转换成JSON string，并保存到文件中
    /// </summary>
    class ExportJson
    {
        DataTable mSheet;
        List<Dictionary<string, object>> mJsonDataList;

        public ExportJson(DataTable sheet)
        {
            mSheet = sheet;
            mJsonDataList = new List<Dictionary<string, object>>();
        }

        /// <summary>
        /// 导出为json格式数据
        /// </summary>
        public void Export()
        {
            if (mSheet.Columns.Count <= 0 || mSheet.Rows.Count <= 0)
                return;

            mJsonDataList.Clear();

            // 类型
            Dictionary<string, string> dicTypes = new Dictionary<string, string>();
            DataRow typeRow = mSheet.Rows[0];
            DataRow commentRow = mSheet.Rows[1];
            foreach (DataColumn column in mSheet.Columns)
            {
                dicTypes.Add(column.ToString(), typeRow[column].ToString());
            }
               

            //以第一列为ID，转换成ID->Object的字典
            for (int i = 2; i < mSheet.Rows.Count; i++)
            {
                DataRow row = mSheet.Rows[i];

                if (row[mSheet.Columns[0]].ToString().Length <= 0)
                    continue;

                var rowData = new Dictionary<string, object>();
                for (int c = 0; c < mSheet.Columns.Count; c++)
                {
                    var column = mSheet.Columns[c];
                    object value = row[column];
                    // 去掉数值字段的“.0”
                    if (value.GetType() == typeof(double))
                    {
                        double num = (double)value;
                        if ((int)num == num)
                            value = (int)num;
                    }

                    // 注释列
                    if (string.IsNullOrEmpty(dicTypes[column.ToString()]))
                        continue;

                    if (dicTypes[column.ToString()] == "string")
                    {
                        value = value.ToString();
                    }
                    else if (dicTypes[column.ToString()] == "float")
                    {
                        //value = (float)value;
                        string strTemp = value.ToString();
                        if (strTemp.Equals(""))
                            value = float.Parse("0");
                        else
                            value = float.Parse(value.ToString());
                    }

                    string fieldName = column.ToString(); // 表头自动转换成小写
                    if (!string.IsNullOrEmpty(fieldName))
                        if (!value.GetType().FullName.Equals("System.DBNull"))
                            rowData[fieldName] = value;
                }

                mJsonDataList.Add(rowData);
            }
        }
        
        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="filePath">输出文件路径</param>
        /// <param name="encoding"></param>
        public void SaveToFile(string filePath, Encoding encoding)
        {
            if (mJsonDataList.Count <= 0)
                throw new Exception("JsonExporter内部数据为空。");

            //保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                {
                    //转换为JSON字符串
                    writer.Write(JsonMapper.ToJson(mJsonDataList));
                }
            }
        }
    }
}

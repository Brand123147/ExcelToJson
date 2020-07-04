using System;
using System.IO;
using System.Text;
using System.Data;
using System.Collections.Generic;


namespace Excel2Json
{
    /// <summary>
    /// 根据表头，生成C#类定义数据结构
    /// 表头使用三行定义：字段名称、字段类型、注释
    /// </summary>
    class ExportCSharp
    {
        /// <summary>
        /// 字段
        /// </summary>
        struct FieldInfo
        {
            /// <summary>
            /// 变量名
            /// </summary>
            public string name;
            /// <summary>
            /// 类型
            /// </summary>
            public string type;
            /// <summary>
            /// 注释
            /// </summary>
            public string notes;
        }

        DataTable mSheet;
        List<FieldInfo> mFieldInfoList;

        public ExportCSharp(DataTable sheet)
        {
            mSheet = sheet;
        }

        //导出为CSharp
        public bool Export()
        {
            //First Row as Column Name
            if (mSheet.Rows.Count < 2)
                return false;

            mFieldInfoList = new List<FieldInfo>();
            DataRow typeRow = mSheet.Rows[0];
            DataRow notesRow = mSheet.Rows[1];

            for (int i = 0; i < mSheet.Columns.Count; i++)
            {
                var column = mSheet.Columns[i];
                if (column.ToString().Length <= 0 || typeRow[column].ToString().Length <= 0) continue;
                if (column.ToString().Length >= 6)
                {
                    string co = column.ToString().Substring(0, 6);
                    if (co == "Column")
                    {
                        continue;
                    }
                }

                FieldInfo field;
                field.name = column.ToString();
                field.type = typeRow[column].ToString();
                field.notes = notesRow[column].ToString();
                mFieldInfoList.Add(field);
            }

            return true;
        }

        public void SaveToFile(string strTemplatePath, string filePath, Encoding encoding)
        {
            // 加载Excel文件
            using (FileStream templateFile = File.Open(strTemplatePath, FileMode.Open, FileAccess.Read))
            {
                if (mFieldInfoList == null)
                    throw new Exception("FieldInfo结构中数据为空（字段为空）。");

                byte[] templateByte = new byte[templateFile.Length];
                templateFile.Read(templateByte, 0, templateByte.Length);
                string strTemplate = Encoding.UTF8.GetString(templateByte);

                string tableName = Path.GetFileNameWithoutExtension(filePath);

                // 表格字段构建
                StringBuilder sbClassField = new StringBuilder();
                for (int i = 0; i < mFieldInfoList.Count; ++i)
                {
                    var field = mFieldInfoList[i];

                    if (i == 0)
                        sbClassField.AppendFormat("/// <summary> \n \t\t/// {2}\n \t\t/// </summary> \n \t\tpublic {0} {1};", field.type, field.name, field.notes);
                    else
                        sbClassField.AppendFormat("\t\t/// <summary> \n \t\t/// {2}\n \t\t/// </summary> \n \t\tpublic {0} {1};", field.type, field.name, field.notes);

                    if (i != mFieldInfoList.Count - 1)
                        sbClassField.AppendLine();
                }

                var a = sbClassField.ToString();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(strTemplate, tableName, a);

                //保存文件
                string newFilePath = filePath.Substring(0, filePath.Length - 3) + "Cfg.cs";
                using (FileStream file = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (TextWriter writer = new StreamWriter(file, encoding))
                        writer.Write(sb.ToString());
                }
            }
        }
    }
}
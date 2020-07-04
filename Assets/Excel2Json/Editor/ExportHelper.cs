using Excel;
using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Excel2Json
{
    /// <summary>
    /// 导出帮助类
    /// </summary>
    class ExportHelper
    {
        string mPathJson;
        string mPathCSharp;
        string mPathExcel;
        string mPathTemplate;
        float mProgress = 0.0f;

        List<string> mListJson;
        List<string> mListCSharp;

        [MenuItem("Tools/表格导出")]
        static void NewMenuOption()
        {
            (new ExportHelper()).Run();
        }

        public ExportHelper()
        {
            mPathExcel = Application.dataPath + @"//../PlanningData/";
            mPathJson = Application.dataPath + @"\_Config\JsonData";
            mPathCSharp = Application.dataPath + @"\_Config\CSharpData";
            mPathTemplate = Application.dataPath + @"\Excel2Json\Template\CSharpTemplate.txt";
            mListJson = new List<string>();
            mListCSharp = new List<string>();
        }

        public void Run()
        {
            // 遍历文件
            if (!Directory.Exists(mPathExcel))
            {
                Directory.CreateDirectory(mPathExcel);
            }
            var files = (new DirectoryInfo(mPathExcel)).GetFiles();
            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].Extension == ".xlsx" && !IsChinese(files[i].Name) && IsCorrectXLSX(files[i].Name))
                {
                    mProgress = (i + 0.1f) / files.Length;
                    try
                    {
                        Export(files[i]);
                    }
                    catch (Exception exp)
                    {
                        Debug.LogError(string.Format("[{0}]: {1}", files[i].Name, exp.Message));
                    }
                }
            }
            ClearInvalidFile();
            EditorUtility.ClearProgressBar();
            EditorSceneManager.MarkAllScenesDirty();
        }

        private void ClearInvalidFile()
        {
            // 遍历文件
            if (!Directory.Exists(mPathJson))
            {
                Directory.CreateDirectory(mPathJson);
            }
            var jsonFolder = new DirectoryInfo(mPathJson);
            foreach (FileInfo f in jsonFolder.GetFiles())
            {
                if (f.Extension == ".json" && mListJson.Find(x => x == f.Name) == null)
                {
                    Debug.Log("Delete File: " + f.Name);
                    File.Delete(f.FullName);
                }
            }
            if (!Directory.Exists(mPathCSharp))
            {
                Directory.CreateDirectory(mPathCSharp);
            }
            var csharpFolder = new DirectoryInfo(mPathCSharp);
            foreach (FileInfo f in csharpFolder.GetFiles())
            {
                if (f.Extension == ".cs" && mListCSharp.Find(x => x == f.Name) == null)
                {
                    File.Delete(f.FullName);
                    Debug.Log("Delete File: " + f.Name);
                }
            }
        }

        // 筛选是否是汉语文件名若是汉语则去除
        private bool IsChinese(string text)
        {
            for (int i = 0; i < text.Length; ++i)
                if ((int)text[i] > 127)
                    return true;
            return false;
        }
        /// <summary>
        /// 表格是否开着,需要关闭才能导出
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsCorrectXLSX(string text)
        {
            if (text[0] == '~')
                return false;
            return true;
        }

        private void CreateOrDeleteDirectory(string path)
        {
            if ((new DirectoryInfo(path)).Exists)
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 导出到json文件中
        /// </summary>
        private void Export(FileInfo fileInfo)
        {
            var tableName = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(fileInfo.Extension));

            // 加载Excel文件
            using (FileStream excelFile = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet book = excelReader.AsDataSet();

                // 数据检测
                if (book.Tables.Count < 1)
                    throw new Exception("Excel文件中没有找到Sheet: " + tableName);

                // 取得数据
                DataTable sheet = book.Tables[0];   // 这里只读取第一张sheet表，读取其他sheet以此类推
                if (sheet.Rows.Count <= 0)
                    throw new Exception("Excel Sheet中没有数据: " + tableName);

                // 确定编码
                Encoding encode = new UTF8Encoding(false);

                // 导出json
                ExportJson expJson = new ExportJson(sheet);
                expJson.Export();
                expJson.SaveToFile(string.Format("{0}/{1}.json", mPathJson, tableName), encode);
                mListJson.Add(tableName + ".json");

                // 通过Template模版文件导出C#
                ExportCSharp expCSharp = new ExportCSharp(sheet);
                expCSharp.Export();
                expCSharp.SaveToFile(mPathTemplate, string.Format("{0}/{1}.cs", mPathCSharp, tableName), encode);
                mListCSharp.Add(tableName + "Cfg.cs");
            }
            EditorUtility.DisplayProgressBar("导出表格完成", fileInfo.Name, mProgress);
        }
    }
}

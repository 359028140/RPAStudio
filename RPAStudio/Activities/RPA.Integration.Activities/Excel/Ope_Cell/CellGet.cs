﻿using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using Excel = Microsoft.Office.Interop.Excel;


namespace RPA.Integration.Activities.ExcelPlugins
{
    [Designer(typeof(CellGetDesigner))]
    public sealed class CellGet : AsyncCodeActivity
    {
        public CellGet()
        {
        }

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/RPA.Integration.Activities;Component/Resources/Excel/getcell.png"; } }

        InArgument<Int32> _CellRow = 1;
        [Localize.LocalizedCategory("Category30")] //单元格选项 //Cell Options //セルオプション
        [OverloadGroup("CellColAndRow")]
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName16")] //行 //Row //行
        [Browsable(true)]
        public InArgument<Int32> CellRow
        {
            get
            {
                return _CellRow;
            }
            set
            {
                _CellRow = value;
            }
        }

        InArgument<Int32> _CellColumn = 1;
        [Localize.LocalizedCategory("Category30")] //单元格选项 //Cell Options //セルオプション
        [OverloadGroup("CellColAndRow")]
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName17")] //列 //Column //コラム
        [Browsable(true)]
        public InArgument<Int32> CellColumn
        {
            get
            {
                return _CellColumn;
            }
            set
            {
                _CellColumn = value;
            }
        }

        [Localize.LocalizedCategory("Category30")] //单元格选项 //Cell Options //セルオプション
        [OverloadGroup("CellName")]
        [Localize.LocalizedDescription("Description76")] //代表单元格名称的VB表达式，如A1 //VB expression representing the cell name, such as A1 //A1などのセル名を表すVB式
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName141")] //单元格名称 //Cell Name //セル名
        [Browsable(true)]
        public InArgument<string> CellName
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category31")] //工作表选项 //Sheet Options //シートオプション
        [Localize.LocalizedDisplayName("DisplayName18")] //工作表名称 //Worksheet name //ワークシート名
        [Browsable(true)]
        [Localize.LocalizedDescription("Description77")] //为空代表当前活动工作表 //Blank represents the currently active worksheet //空白は現在アクティブなワークシートを表します
        public InArgument<string> SheetName
        {
            get;
            set;
        }


        [Localize.LocalizedCategory("Category2")] //输出 //Output //出力
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName19")] //单元格内容 //Cell content //セルコンテンツ
        [Browsable(true)]
        public OutArgument<object> CellContent
        {
            get;
            set;
        }



        [Browsable(false)]
        public string ClassName { get { return "CellGet"; } }
        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }
        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            PropertyDescriptor property = context.DataContext.GetProperties()[ExcelCreate.GetExcelAppTag];
            Excel::Application excelApp = property.GetValue(context.DataContext) as Excel::Application;
            try
            {
                Int32 cellColumn = CellColumn.Get(context);
                Int32 cellRow = CellRow.Get(context);
                string cellName = CellName.Get(context);
                string sheetName = SheetName.Get(context);
                Excel::_Worksheet sheet;

                if (sheetName == null)
                    sheet = excelApp.ActiveSheet;
                else
                    sheet = excelApp.ActiveWorkbook.Sheets[sheetName];
                object cellContent = null;
                if (cellName == null)
                {
                    cellContent = sheet.Cells[cellRow, cellColumn].Value2;
                }
                else
                {
                    cellContent = sheet.Range[cellName].Value2;
                }
                CellContent.Set(context, cellContent);
                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                sheet = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "EXCEL获取单元格内容出错", e.Message);
                new CommonVariable().realaseProcessExit(excelApp);
            }
            m_Delegate = new runDelegate(Run);
            return m_Delegate.BeginInvoke(callback, state);
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
        }
    }
}

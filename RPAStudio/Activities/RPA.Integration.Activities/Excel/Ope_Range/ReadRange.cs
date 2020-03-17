﻿using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using Excel = Microsoft.Office.Interop.Excel;
namespace RPA.Integration.Activities.ExcelPlugins
{

    [Designer(typeof(ReadRangeDesigner))]
    public sealed class ReadRange : AsyncCodeActivity
    {
        public ReadRange()
        {
        }


        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/RPA.Integration.Activities;Component/Resources/Excel/rangeread.png"; } }



        [Localize.LocalizedCategory("Category4")] //选项 //Option //オプション
        [Localize.LocalizedDisplayName("DisplayName18")] //工作表名称 //Worksheet name //ワークシート名
        [Localize.LocalizedDescription("Description77")] //为空代表当前活动工作表 //Blank represents the currently active worksheet //空白は現在アクティブなワークシートを表します
        [Browsable(true)]
        public InArgument<string> SheetName { get; set; } = "";

        [Localize.LocalizedCategory("Category32")] //单元格起始 //Start Cell //開始セル
        [Localize.LocalizedDisplayName("DisplayName16")] //行 //Row //行
        [Localize.LocalizedDescription("UsedRangeApplied")] //如果为空白，则使用UsedRange //UsedRange is applied if it is blank //空白の場合はUsedRangeが適用されます
        [Browsable(true)]
        public InArgument<Int32> CellRow_Begin
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category32")] //单元格起始 //Start Cell //開始セル
        [Localize.LocalizedDisplayName("DisplayName17")] //列 //Column //コラム
        [Localize.LocalizedDescription("UsedRangeApplied")] //如果为空白，则使用UsedRange //UsedRange is applied if it is blank //空白の場合はUsedRangeが適用されます
        [Browsable(true)]
        public InArgument<Int32> CellColumn_Begin
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category32")] //单元格起始 //Start Cell //開始セル
        [Localize.LocalizedDescription("Description76")] //代表单元格名称的VB表达式，如A1 //VB expression representing the cell name, such as A1 //A1などのセル名を表すVB式
        [Localize.LocalizedDisplayName("DisplayName141")] //单元格名称 //Cell Name //セル名
        [Browsable(true)]
        public InArgument<string> CellName_Begin
        {
            get; set;
        }

        [Localize.LocalizedCategory("Category34")] //单元格终点 //Cell end //セルエンド
        [Localize.LocalizedDisplayName("DisplayName16")] //行 //Row //行
        [Localize.LocalizedDescription("UsedRangeApplied")] //如果为空白，则使用UsedRange //UsedRange is applied if it is blank //空白の場合はUsedRangeが適用されます
        [Browsable(true)]
        public InArgument<Int32> CellRow_End
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category34")] //单元格终点 //Cell end //セルエンド
        [Localize.LocalizedDisplayName("DisplayName17")] //列 //Column //コラム
        [Localize.LocalizedDescription("UsedRangeApplied")] //如果为空白，则使用UsedRange //UsedRange is applied if it is blank //空白の場合はUsedRangeが適用されます
        [Browsable(true)]
        public InArgument<Int32> CellColumn_End
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category34")] //单元格终点 //Cell end //セルエンド
        [Localize.LocalizedDescription("Description78")] //代表单元格名称的VB表达式，如B2 //VB expression representing the cell name, such as B2 //B2などのセル名を表すVB式
        [Localize.LocalizedDisplayName("DisplayName141")] //单元格名称 //Cell Name //セル名
        [Browsable(true)]
        public InArgument<string> CellName_End
        {
            get; set;
        }

        [Localize.LocalizedCategory("Category4")] //选项 //Option //オプション
        [Localize.LocalizedDisplayName("DisplayName154")] //表头 //Header //ヘッダー
        [Browsable(true)]
        public bool isTitle
        {
            get;set;
        }

        [Localize.LocalizedCategory("Category2")] //输出 //Output //出力
        [RequiredArgument]
        [DisplayName("DataTable")]
        [Browsable(true)]
        [Localize.LocalizedDescription("Description19")] //存储读取数据的DataTable //Store a DataTable that reads data //データを読み取るDataTableを保存する
        public OutArgument<System.Data.DataTable> DataTable
        {
            get;
            set;
        }


        [Browsable(false)]
        public string ClassName { get { return "ReadRange"; } }
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
                //string sheetName = SheetName.Get(context);
                //string cellName_Begin = CellName_Begin.Get(context);
                //string cellName_End = CellName_End.Get(context);
                //int cellRow_Begin = CellRow_Begin.Get(context);
                //int cellColumn_Begin = CellColumn_Begin.Get(context);
                //int cellRow_End = CellRow_End.Get(context);
                //int cellColumn_End = CellColumn_End.Get(context);

                //Excel::_Worksheet sheet;
                //if (sheetName != null)
                //    sheet = excelApp.ActiveWorkbook.Sheets[sheetName];
                //else
                //    sheet = excelApp.ActiveSheet;

                //Excel::Range range1, range2;
                //range1 = cellName_Begin == null ? sheet.Cells[cellRow_Begin, cellColumn_Begin] : sheet.Range[cellName_Begin];
                //range2 = cellName_End == null ? sheet.Cells[cellRow_End, cellColumn_End] : sheet.Range[cellName_End];
                Excel::_Worksheet sheet;
                Excel::Range range1, range2;
                RangeFunction.GetRange(excelApp, context, SheetName, CellName_Begin, CellName_End,
                                       CellRow_Begin, CellColumn_Begin, CellRow_End, CellColumn_End,
                                       out sheet, out range1, out range2);
                Excel::Range range3 = sheet.Range[range1, range2];
                System.Data.DataTable dt = new System.Data.DataTable();
                int iRowCount = range3.Rows.Count;
                int iColCount = range3.Columns.Count;
                int rowBegin = range3.Row;
                int colBegin = range3.Column;

                //生成列头
                for (int i = 0; i < iColCount; i++)
                {
                    var name = "column" + i;
                    if (isTitle)
                    {
                        var txt = ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[rowBegin, i + colBegin]).Text.ToString();
                        if (!string.IsNullOrEmpty(txt))
                            name = txt;
                    }
                    while (dt.Columns.Contains(name))
                        name = name + "_1";//重复行名称会报错。
                    dt.Columns.Add(new System.Data.DataColumn(name, typeof(string)));
                }
                //生成行数据
                Microsoft.Office.Interop.Excel.Range range;
                int rowIdx = isTitle ? 2 : 1;
                for (int iRow = rowIdx; iRow <= iRowCount; iRow++)
                {
                    System.Data.DataRow dr = dt.NewRow();
                    for (int iCol = 1; iCol <= iColCount; iCol++)
                    {
                        range = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[iRow + rowBegin - 1 , iCol + colBegin - 1];
                        dr[iCol - 1] = (range.Value2 == null) ? "" : range.Text.ToString();
                    }
                    dt.Rows.Add(dr);
                }
                DataTable.Set(context, dt);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                sheet = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "EXCEL读取区域执行过程出错", e.Message);
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

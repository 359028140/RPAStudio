﻿using System.Activities;
using System.ComponentModel;
using System.Data;

namespace RPA.Core.Activities.DataTableActivity
{
    [Designer(typeof(MergeDataTableDesigner))]
    public sealed class MergeDataTable : CodeActivity
    {
        public MergeDataTable()
        {
            
        }

        public new string DisplayName
        {
            get
            {
                return "MergeDataTable";
            }
        }


        [Category("输入")]
        [RequiredArgument]
        [DisplayName("目标")]
        [Description("合并源DataTable的DataTable对象")]
        public InArgument<DataTable> Destination { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("源")]
        [Description("要添加到目标DataTable的DataTable对象")]
        public InArgument<DataTable> Source { get; set; }


        public MissingSchemaAction _MergeType = MissingSchemaAction.Add;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("合并操作")]
        [Description("指定合并两个DataTable时要执行的操作")]
        public MissingSchemaAction MergeType
        {
            get
            {
                return _MergeType;
            }
            set
            {
                _MergeType = value;
            }
        }


        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/RPA.Core.Activities;Component/Resources/DataTable/datatable.png";
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }


        protected override void Execute(CodeActivityContext context)
        {
            DataTable destination = Destination.Get(context);
            DataTable source = Source.Get(context);

            destination.Merge(source, true, MergeType);
        }
    }
}
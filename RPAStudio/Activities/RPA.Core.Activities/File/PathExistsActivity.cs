﻿using Plugins.Shared.Library;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.IO;

namespace RPA.Core.Activities.FileActivity
{
    [Designer(typeof(PathExistsDesigner))]
    public sealed class PathExistsActivity : CodeActivity
    {
        public new string DisplayName
        {
            get
            {
                return "Path Exists";
            }
        }

        [RequiredArgument]
        [Category("Input")]
        [Description("要检查的完整路径。")]
        public InArgument<string> Path { get; set; }

        [Category("Input")]
        public int PathType { get; set; }

        [Category("输出")]
        [Description("声明是否找到文档或文件。")]
        public OutArgument<Boolean> Exists { get; set; }

        static PathExistsActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(PathExistsActivity), "PathType", new EditorAttribute(typeof(PathTypeEditor), typeof(PathTypeEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string _Path = Path.Get(context);
                Boolean _Exists = false;
                if (PathType == (int)PathTypeEditor.PathTypeEnum.File)            //
                {
                     _Exists = File.Exists(_Path);
                }
                else
                {
                    _Exists = Directory.Exists(_Path);
                }

                Exists.Set(context, _Exists);
            }
            catch
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个错误产生", "判断路径是否存在出现异常!");
            }
        }
    }
}

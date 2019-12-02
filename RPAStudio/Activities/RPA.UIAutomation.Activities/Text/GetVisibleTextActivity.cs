﻿using System;
using System.Activities;
using System.ComponentModel;
using System.Windows;

namespace RPA.UIAutomation.Activities.Text
{
    [Designer(typeof(GetVisibleTextDesigner))]
    public sealed class GetVisibleTextActivity : CodeActivity
    {
        public new string DisplayName;
        [Browsable(false)]
        public string _DisplayName { get { return "Get Visible Text"; } }

        [Category("UI对象")]
        [OverloadGroup("G1")]
        [RequiredArgument]
        [Browsable(true)]
        [DisplayName("窗口指示器")]
        [Description("用于在执行活动时查找特定UI元素的Text属性")]
        public InArgument<string> Selector { get; set; }

        [Category("UI对象")]
        [OverloadGroup("G2")]
        [RequiredArgument]
        [Browsable(true)]
        [DisplayName("UI元素")]
        [Description("要关闭的窗口。该字段仅接受Window变量")]
        public InArgument<UIElement> ActiveWindow { get; set; }


        //  [RequiredArgument]
        [Category("Common")]
        [Description("指定即使当前活动失败，也要继续执行其余的活动。只支持布尔值(True,False)。")]
        public InArgument<bool> ContinueOnError { get; set; }

        [Category("选项")]
        [DisplayName("忽略隐藏")]
        [Description("如果选中此复选框，则不提取指定ui元素上的字符串信息，默认情况下不选中此复选框")]
        public bool IgnoreHidden { get; set; }

        [Category("选项")]
        [DisplayName("分隔符")]
        [Description("指定用作字符串先锋的字符。如果字段为空，则使用所有已知的文本分隔符(空格、句号、逗号等)")]
        public InArgument<string> Separator { get; set; }

        [Category("输出")]
        [DisplayName("单词信息")]
        [Description("在指定的ui元素中找到的每个单词的屏幕坐标")]
        public InArgument<UIElement> WordsInfo { get; set; }

        [Category("输出")]
        [DisplayName("文本")]
        [Description("要单击的文本")]
        public InArgument<String> Text { get; set; }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/RPA.UIAutomation.Activities;Component/Resources/Text/gettext.png";
            }
        }

        private System.Windows.Visibility visi = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility visibility
        {
            get
            {
                return visi;
            }
            set
            {
                visi = value;
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
        }
    }
}

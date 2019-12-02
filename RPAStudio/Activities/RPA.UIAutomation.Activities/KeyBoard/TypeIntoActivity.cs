﻿using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Activities.Presentation.PropertyEditing;
using RPA.UIAutomation.Activities.Mouse;
using Plugins.Shared.Library;
using Plugins.Shared.Library.UiAutomation;
using System.Threading;
using System.Windows.Forms;

namespace RPA.UIAutomation.Activities.Keyboard
{
    [Designer(typeof(TypeIntoDesigner))]
    public sealed class TypeIntoActivity : CodeActivity
    {
        public new string DisplayName;
        [Browsable(false)]
        public string _DisplayName { get { return "Type Into"; } }

        [Category("UI对象")]
        [OverloadGroup("G1")]
        [Browsable(true)]
        [DisplayName("窗口指示器")]
        [Description("用于在执行活动时查找特定UI元素的Text属性")]
        public InArgument<string> Selector { get; set; }
        [Category("UI对象")]
        [OverloadGroup("G1")]
        [Browsable(true)]
        [DisplayName("UI元素")]
        [Description("输入UIElement")]
        public InArgument<UiElement> Element { get; set; }

        [Category("Common")]
        [Description("指定即使当前活动失败，也要继续执行其余的活动。只支持布尔值(True,False)。")]
        public InArgument<bool> ContinueOnError { get; set; }

        [Category("Common")]
        [Description("执行活动后的延迟时间(以毫秒为单位)。默认时间为300毫秒。")]
        public InArgument<Int32> DelayAfter { get; set; }

        [Category("Common")]
        [Description("延迟活动开始执行任何操作之前的时间(以毫秒为单位)。默认时间为300毫秒。")]
        public InArgument<Int32> DelayBefore { get; set; }

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/RPA.UIAutomation.Activities;Component/Resources/KeyBoard/text.png"; } }

        [Browsable(false)]
        public List<string> KeyTypes
        {
            get
            {
                KeyboardTypes key = new KeyboardTypes();
                return key.getKeyTypes;
            }
        }

        [RequiredArgument]
        [Category("输入项")]
        public InArgument<string> Text { get; set; }

        [Category("鼠标选项")]
        public bool isRunClick { get; set; }


        [Category("鼠标选项")]
        public Int32 ClickType { get; set; }
        [Category("鼠标选项")]
        public Int32 MouseButton { get; set; }

        [Category("鼠标选项")]
        public InArgument<Int32> offsetX { get; set; }
        [Category("鼠标选项")]
        public InArgument<Int32> offsetY { get; set; }
        [Category("鼠标选项")]
        [DisplayName("使用坐标点")]
        public bool usePoint { get; set; }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

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

        static TypeIntoActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(TypeIntoActivity), "ClickType", new EditorAttribute(typeof(MouseClickTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(TypeIntoActivity), "MouseButton", new EditorAttribute(typeof(MouseButtonTypeEditor), typeof(PropertyValueEditor)));
         //   builder.AddCustomAttributes(typeof(TypeIntoActivity), "KeyModifiers", new EditorAttribute(typeof(KeyModifiersEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        void ParseStringToList(ref string inText, ref List<string> strList)
        {
            string strBuff = "";
            string keyBuff = "";
            bool isKeyFlag = false;
            for (int counter = 0; counter < inText.Length; counter++)
            {
                if (counter < inText.Length - 1)
                {
                    if (inText[counter] == '[' && inText[counter+1] == 'k')
                    {
                        isKeyFlag = true;
                    }
                    if (inText[counter] == ')' && inText[counter+1] == ']')
                    {
                        isKeyFlag = false;
                    }
                }
                if (isKeyFlag)
                {
                    keyBuff += inText[counter].ToString();
                    if (strBuff != "")
                    {
                        strBuff = strBuff.Replace("[k(", "");
                        strBuff = strBuff.Replace(")]", "");
                        strList.Add(strBuff);
                        strBuff = "";
                    }
                }
                else
                {
                    strBuff += inText[counter].ToString();

                    if (keyBuff != "")
                    {
                        keyBuff = keyBuff.Replace("[k(", "");
                        keyBuff = keyBuff.Replace("[k(", "");
                        keyBuff = "[(" + keyBuff + ")]";
                        strList.Add(keyBuff);
                        keyBuff = "";
                    }
                }

                if(counter == inText.Length - 1 && inText[counter] != ']')
                {
                    strBuff = strBuff.Replace("[k(", "");
                    strBuff = strBuff.Replace(")]", "");
                    strList.Add(strBuff);
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            Int32 _delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Int32 _delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 300);
            Thread.Sleep(_delayBefore);

            try
            {
                var selStr = Selector.Get(context);
                UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                if (element == null && selStr != null)
                {
                    element = UiElement.FromSelector(selStr);
                }
               
                Int32 pointX = 0;
                Int32 pointY = 0;
                if (usePoint)
                {
                    pointX = offsetX.Get(context);
                    pointY = offsetY.Get(context);
                }
                else
                {
                    if (element != null)
                    {
                        pointX = element.GetClickablePoint().X;
                        pointY = element.GetClickablePoint().Y;
                        //element.SetForeground();//输入框置前窗口会导致焦点跳出，去除
                    }
                }
                string expValue = Text.Get(context);
                List<string> strList = new List<string>();
                ParseStringToList(ref expValue, ref strList);
                if (isRunClick)
                {
                    UiElement.MouseMoveTo(pointX, pointY);
                    UiElement.MouseAction((Plugins.Shared.Library.UiAutomation.ClickType)ClickType, (Plugins.Shared.Library.UiAutomation.MouseButton)MouseButton);
                }
                foreach (string _strValue in strList)
                {
                    string strValue = _strValue;
                    if (strValue.Contains("[(") && strValue.Contains(")]"))
                    {
                        strValue = strValue.Replace("[(", "");
                        strValue = strValue.Replace(")]", "");
                        Thread.Sleep(100);
                        if(Common.DealVirtualKeyPress(strValue.ToUpper()))
                        {
                            Common.DealVirtualKeyRelease(strValue.ToUpper());
                        }
                        else
                        {
                            SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个错误产生", "找不到键值");
                            if (ContinueOnError.Get(context))
                            {
                                return;
                            }
                            else
                            {
                                throw new NotImplementedException("找不到键值");
                            }
                        }
                    }
                    else if (strValue != null && strValue != "")
                    {
                        Thread.Sleep(100);
                        SendKeys.SendWait(strValue);
                    }
                }
                Thread.Sleep(_delayAfter);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个错误产生", e.Message);
                if (ContinueOnError.Get(context))
                {
                    return;
                }
                else
                {
                    throw new NotImplementedException(e.Message);
                }
            }
        }
    }
}

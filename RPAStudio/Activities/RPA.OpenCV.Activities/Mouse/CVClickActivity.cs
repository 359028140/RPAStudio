﻿using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;

namespace RPA.OpenCV.Activities.Mouse
{
    [Designer(typeof(CVActivityDesigner))]
    public sealed class CVClickActivity : CodeActivity
    {
        public new string DisplayName;
        [Browsable(false)]
        public string _DisplayName { get { return "CV Click"; } }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath {
            get {
                return @"pack://application:,,,/RPA.OpenCV.Activities;Component/Resources/Mouse/cvclick.png";
            }
        }

        [Localize.LocalizedCategory("Ctg_Public")]
        [Localize.LocalizedDisplayName("DN_ErrorExecution")]
        [Localize.LocalizedDescription("DS_ErrorExecution")]
        public InArgument<bool> ContinueOnError { get; set; }

        [Localize.LocalizedCategory("Ctg_Input")]
        [Browsable(true)]
        [Localize.LocalizedDisplayName("DN_WindowTitle")]
        [Localize.LocalizedDescription("DS_WindowTitle")]
        public InArgument<string> Title {
            get;
            set;
        }

        [Localize.LocalizedCategory("Ctg_Input")]
        [Browsable(true)]
        [Localize.LocalizedDisplayName("DN_MatchingThreshold")]
        [Localize.LocalizedDescription("DS_MatchingThreshold")]
        public InArgument<int> MatchingThreshold { get; set; } = 91;

        [Localize.LocalizedCategory("Ctg_Input")]
        [Browsable(true)]
        [Localize.LocalizedDisplayName("DN_MatchingInterval")]
        [Localize.LocalizedDescription("DS_MatchingInterval")]
        public InArgument<int> MatchingInterval { get; set; } = 2000;

        [Localize.LocalizedCategory("Ctg_Input")]
        [Browsable(true)]
        [Localize.LocalizedDisplayName("DN_Retry")]
        [Localize.LocalizedDescription("DS_Retry")]
        public InArgument<int> Retry { get; set; } = 600;

        [Localize.LocalizedCategory("Ctg_Output")]
        [Browsable(true)]
        public OutArgument<bool> Result { get; set; }

        private System.Windows.Visibility visi = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility visibility {
            get {
                return visi;
            }
            set {
                visi = value;
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string title = Title.Get(context);
                int matchingThreshold = MatchingThreshold.Get(context);
                int matchingInterval = MatchingInterval.Get(context);
                int retry = Retry.Get(context);

                var ret = Common.CVMouseAction(SourceImgPath, FlaxCV.CvActionType.LClick, title, matchingThreshold, matchingInterval, retry);
                Result.Set(context, ret.IsMatched);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "Error on Executing CVClickActivity()", e.Message);
                if (ContinueOnError.Get(context))
                {
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

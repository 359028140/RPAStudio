﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Plugins.Shared.Library;

namespace RPA.Integration.Activities.Mail
{
    [Designer(typeof(SaveOutlookAttachementsDesigner))]
    public sealed class SaveOutlookAttachementsActivity : AsyncCodeActivity
    {
        [Category("Input")]
        [DisplayName("保存路径")]
        [Description("附件的保存路径")]
        public InArgument<string> FolderPath
        {
            get;
            set;
        }

        [Category("Input")]
        [DisplayName("邮件对象")]
        [Description("指定邮件对象")]
        [RequiredArgument]
        public InArgument<MailMessage> Message
        {
            get;
            set;
        }

        [Category("Output")]
        [DisplayName("附件列表")]
        [Description("输出附件列表")]
        public OutArgument<IEnumerable<string>> Attachments
        {
            get;
            set;
        }

        [Category("Options")]
        [DisplayName("过滤器")]
        [Description("正则表达式过滤器，只有符合过滤条件的附件名才会被保存")]
        [DefaultValue(null)]
        public InArgument<string> Filter
        {
            get;
            set;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            context.UserState = cts;
            Task<IEnumerable<string>> task = SaveAttachments(context, cts.Token);
            TaskCompletionSource<IEnumerable<string>> tcs = new TaskCompletionSource<IEnumerable<string>>(state);
            task.ContinueWith(delegate (Task<IEnumerable<string>> t)
            {
                if (cts.Token.IsCancellationRequested || t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
                callback?.Invoke(tcs.Task);
            });
            return tcs.Task;
        }

        Task<IEnumerable<string>> SaveAttachments(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            string folderPath = FolderPath.Get(context);
            MailMessage mail = Message.Get(context);
            string filter = Filter.Get(context);
            return Task.Factory.StartNew((Func<IEnumerable<string>>)delegate
            {
                List<string> list = new List<string>();
                List<Exception> list2 = new List<Exception>();
                if (mail.Attachments == null || mail.Attachments.Count == 0)
                {
                    return list;
                }
                if (string.IsNullOrEmpty(folderPath))
                {
                    folderPath = Directory.GetCurrentDirectory();
                }
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                foreach (Attachment attachment in mail.Attachments)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return list;
                        }
                        string text = Path.GetFileName(attachment.Name);
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            text = Path.Combine(folderPath, text);
                        }
                        if (string.IsNullOrEmpty(filter) || Regex.IsMatch(text, filter))
                        {
                            try
                            {
                                FileStream fileStream = File.Open(text, FileMode.Create, FileAccess.Write);
                                attachment.ContentStream.Position = 0L;
                                attachment.ContentStream.CopyTo(fileStream);
                                fileStream.Close();
                                list.Add(text);
                            }
                            finally
                            {
                                attachment.ContentStream.Position = 0L;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个异常产生", ex.Message);
                        list2.Add(ex);
                    }
                }
                if (list2.Count > 0)
                {
                    throw new AggregateException(list2);
                }
                return list;
            });
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<IEnumerable<string>> task = (Task<IEnumerable<string>>)result;
            try
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }
                if (task.IsCanceled || context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                }
                else
                {
                    Attachments.Set(context, task.Result);
                }
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            ((CancellationTokenSource)context.UserState).Cancel();
        }
    }
}

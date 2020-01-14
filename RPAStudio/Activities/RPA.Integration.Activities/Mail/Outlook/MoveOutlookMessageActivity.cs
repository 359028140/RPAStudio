﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Mail;

namespace RPA.Integration.Activities.Mail
{
    [Designer(typeof(MoveOutlookMessageDesigner))]
    public sealed class MoveOutlookMessageActivity : AsyncCodeActivity
    {
        [Category("Input")]
        [Localize.LocalizedDisplayName("DisplayName95")] //邮件对象 //Mail object //メールオブジェクト
        [Localize.LocalizedDescription("Description61")] //指定邮件对象 //Specify mail object //メールオブジェクトを指定する
        [RequiredArgument]
        public InArgument<MailMessage> MailMessage
        {
            get;
            set;
        }

        [Category("Input")]
        [Localize.LocalizedDisplayName("DisplayName100")] //账户 //Account //アカウント
        [Localize.LocalizedDescription("Description60")] //Outlook账户名 //Outlook account name //Outlookアカウント名
        public InArgument<string> Account
        {
            get;
            set;
        }

        [Category("Input")]
        [Localize.LocalizedDisplayName("DisplayName99")] //邮件目录 //Mail directory //メールディレクトリ
        [Localize.LocalizedDescription("Description67")] //Outlook邮件目录 //Outlook mail directory //Outlookメールディレクトリ
        [RequiredArgument]
        public InArgument<string> MailFolder
        {
            get;
            set;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state);
            CancellationTokenSource cts = new CancellationTokenSource();
            context.UserState = cts;
            MoveMessageAction(context, cts.Token).ContinueWith(delegate (Task<object> t)
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled || cts.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
                callback?.Invoke(tcs.Task);
            });
            return tcs.Task;
        }

        private Task<object> MoveMessageAction(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            MailMessage message = MailMessage.Get(context);
            string folderpath = MailFolder.Get(context);
            string account = Account.Get(context);
            return Task.Factory.StartNew(((Func<object>)delegate
            {
                OutlookAPI.MoveMessage(message, folderpath, account);
                return null;
            }));
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<object> task = (Task<object>)result;
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
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
        }
    }
}

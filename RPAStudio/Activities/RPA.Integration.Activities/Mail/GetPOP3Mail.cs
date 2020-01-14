﻿using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using MailKit.Security;
using MimeKit;
using System.Collections.Generic;
using MailKit.Net.Pop3;
using System.Activities.Statements;

namespace RPA.Integration.Activities.Mail
{
    [Designer(typeof(GetPOP3MailDesigner))]
    public sealed class GetPOP3Mail : NativeActivity
    {
        public new string DisplayName
        {
            get
            {
                return "GetPOP3Mail";
            }
        }

        [Browsable(false)]
        public ActivityAction<object, object, object[]> Body { get; set; }


        public GetPOP3Mail()
        {
            Body = new ActivityAction<object, object, object[]>
            {
                Argument1 = new DelegateInArgument<object>(GetMailList),
                Argument2 = new DelegateInArgument<object>(GetClient),
                Argument3 = new DelegateInArgument<object[]>(GetConfig),
                Handler = new Sequence()
                {
                    DisplayName = "Do"
                }
            };
        }


        public static string GetMailList { get { return "GetMail"; } }
        public static string GetClient { get { return "GetClient"; } }
        public static string GetConfig { get { return "GetConfig"; } }


        [Localize.LocalizedCategory("Category16")] //登录 //Log in //ログイン
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName64")] //邮件账户 //Mail account //メールアカウント
        [Localize.LocalizedDescription("Description21")] //用于发送邮件的电子邮件帐户 //Email account used to send mail //メールの送信に使用されるメールアカウント
        public InArgument<string> Email { get; set; }
        [Localize.LocalizedCategory("Category16")] //登录 //Log in //ログイン
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName57")] //密码 //password //パスワード
        [Localize.LocalizedDescription("Description22")] //用于发送邮件的电子邮件帐户的密码 //The password for the email account used to send the message //メッセージの送信に使用される電子メールアカウントのパスワード
        public InArgument<string> Password { get; set; }


        [Localize.LocalizedCategory("DisplayName54")] //主机 //Host //ホスト
        [RequiredArgument]
        [Localize.LocalizedDisplayName("Category13")] //服务器 //server //サーバー
        [Localize.LocalizedDescription("Description23")] //使用的电子邮件服务器主机 //Email server host used //使用されているメールサーバーホスト
        public InArgument<string> Server { get; set; }
        [Localize.LocalizedCategory("DisplayName54")] //主机 //Host //ホスト
        [Localize.LocalizedDisplayName("DisplayName55")] //端口 //port //港
        [Localize.LocalizedDescription("Description24")] //电子邮件将通过的端口 //The port that the email will pass through //電子メールが通過するポート
        public InArgument<Int32> Port { get; set; }



        [Localize.LocalizedCategory("Category4")] //选项 //Option //オプション
        [Localize.LocalizedDisplayName("DisplayName66")] //安全连接 //Secure connection //安全な接続
        [Localize.LocalizedDescription("Description26")] //指定用于连接的SSL和/或TLS加密 //Specify SSL and/or TLS encryption for the connection //接続にSSLおよび/またはTLS暗号化を指定する
        public SecureSocketOptions SecureConnection { get; set; }
        [Localize.LocalizedCategory("Category4")] //选项 //Option //オプション
        [RequiredArgument]
        [Localize.LocalizedDisplayName("DisplayName67")] //检索消息数 //Retrieve the number of messages //メッセージの数を取得する
        [Localize.LocalizedDescription("Description27")] //从列表顶部开始检索的消息数 //Number of messages retrieved from the top of the list //リストの先頭から取得されたメッセージの数
        public InArgument<Int32> Counts { get; set; }
        [Localize.LocalizedCategory("Category4")] //选项 //Option //オプション
        [Localize.LocalizedDisplayName("DisplayName68")] //标记消息删除 //Tag message deletion //タグメッセージの削除
        [Localize.LocalizedDescription("Description28")] // 指定是否应将读取的消息标记为删除 //Specifies whether the read message should be marked for deletion //読み取りメッセージに削除のマークを付けるかどうかを指定します
        public bool DeleteMessages { get; set; }


        [Localize.LocalizedCategory("Category2")] //输出 //Output //出力
        [Localize.LocalizedDisplayName("DisplayName75")] //消息 //Message //メッセージ
        [Localize.LocalizedDescription("Description34")] //将检索到的消息作为MailMessage对象的集合 //Use the retrieved message as a collection of MailMessage objects //取得したメッセージをMailMessageオブジェクトのコレクションとして使用します
        public OutArgument<List<MimeMessage>> MailMsgList { get; set; }


        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/RPA.Integration.Activities;Component/Resources/Mail/GetMail.png";
            }
        }


        [Localize.LocalizedCategory("Category17")] //筛选 //Filter //スクリーニング
        [Localize.LocalizedDisplayName("DisplayName72")] //主题关键字 //Subject keyword //件名キーワード
        [Localize.LocalizedDescription("Description31")] //根据邮件主题筛选相应的邮件 //Filter the corresponding message based on the subject of the message //メッセージの件名に基づいて、対応するメッセージをフィルタリングします
        public InArgument<String> MailTopicKey { get; set; }


        [Localize.LocalizedCategory("Category17")] //筛选 //Filter //スクリーニング
        [Localize.LocalizedDisplayName("DisplayName73")] //发件人关键字 //Sender keyword //送信者キーワード
        [Localize.LocalizedDescription("Description32")] //根据发件人地址筛选相应的邮件 //Filter the corresponding message based on the sender's address //送信者のアドレスに基づいて対応するメッセージをフィルタリングする
        public InArgument<String> MailSenderKey { get; set; }

        [Localize.LocalizedCategory("Category17")] //筛选 //Filter //スクリーニング
        [Localize.LocalizedDisplayName("DisplayName74")] //内容关键字 //Content keyword //コンテンツキーワード
        [Localize.LocalizedDescription("Description33")] //根据邮件超文本内容筛选相应的邮件 //Filter the corresponding message based on the message's hypertext content //メッセージのハイパーテキストコンテンツに基づいて対応するメッセージをフィルター処理する
        public InArgument<String> MailTextBodyKey { get; set; }


        protected override void Execute(NativeActivityContext context)
        {
            string username = Email.Get(context);               //发送端账号   
            string password = Password.Get(context);            //发送端密码(这个客户端重置后的密码)
            string server = Server.Get(context);                //邮件服务器
            Int32 port = Port.Get(context);                     //端口号
            Int32 counts = Counts.Get(context);
            List<object> configList = new List<object>();
            Pop3Client emailClient = new Pop3Client();
            List<MimeMessage> emails = new List<MimeMessage>();


            string mailTopicKey = MailTopicKey.Get(context);
            string mailSenderKey = MailSenderKey.Get(context);
            string mailTextBodyKey = MailTextBodyKey.Get(context);

            try
            {
                emailClient.Connect(server, port, SecureConnection);
                emailClient.Authenticate(username, password);
                for (int i = emailClient.Count - 1, j = 0; i >= 0 && j < counts; i--, j++)
                {
                    MimeMessage message = emailClient.GetMessage(i);

                    InternetAddressList Sender = message.From;
                    string SenderStr = Sender[0].Name;
                    string Topic = message.Subject;


                    if (mailTopicKey != null && mailTopicKey != "")
                    {
                        if(Topic == null || Topic == "")
                        {
                            j--;
                            continue;
                        }
                        if (!Topic.Contains(mailTopicKey))
                        {
                            j--;
                            continue;
                        }
                    }
                    if (mailSenderKey != null && mailSenderKey != "")
                    {
                        if (SenderStr == null || SenderStr == "")
                        {
                            j--;
                            continue;
                        }
                        if (!SenderStr.Contains(mailSenderKey))
                        {
                            j--;
                            continue;
                        }
                    }
                    if (mailTextBodyKey != null && mailTextBodyKey != "")
                    {
                        if (message.TextBody == null || message.TextBody == "")
                        {
                            j--;
                            continue;
                        }
                        if (!message.TextBody.Contains(mailTextBodyKey))
                        {
                            j--;
                            continue;
                        }
                    }

                    emails.Add(message);
                    if (DeleteMessages)
                        emailClient.DeleteMessage(i);
                }
                MailMsgList.Set(context, emails);
                emailClient.Disconnect(true);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.enOutputType.Error, "获取POP3邮件失败", e.Message);
                emailClient.Disconnect(true);
            }


            configList.Add(server);
            configList.Add(port);
            configList.Add(SecureConnection);
            configList.Add(username);
            configList.Add(password);
            configList.Add("");

            if (Body != null)
            {
                object[] buff = configList.ToArray();
                context.ScheduleAction(Body, emails, emailClient, buff);
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TinyStore.Utils
{
    public static class EmailContext
    {
        public class EmailServer
        {
            public string PosterAddress { get; set; }
            public string PosterName { get; set; }
            public string Host { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; } = true;
            public int Timeout { get; set; } = 30;

            public static ConcurrentDictionary<string, EmailServer> Instances { get; set; } =
                new ConcurrentDictionary<string, EmailServer>();


            /// <summary>
            /// 
            /// </summary>
            /// <param name="emailAddress">发邮人</param>
            /// <param name="smtpUsername">帐号</param>
            /// <param name="smtpPassword">密码</param>
            /// <param name="smtpHost">服务主机</param>
            /// <param name="smtpPort">服务端口:默认25,不采用SSL加密,如果采用SSL加密一般为587或者465</param>
            /// <param name="smtpEnableSsl">否SSL加密:默认不加密，如果需要加密则需要对应修改端口号</param>
            /// <param name="timeout">发送超时:秒</param>
            /// <returns></returns>
            public static EmailServer Generate(MailAddress emailAddress, string smtpUsername, string smtpPassword,
                string smtpHost, int smtpPort = 25, bool smtpEnableSsl = false, int timeout = 30)
            {
                return Generate(emailAddress.Address, emailAddress.DisplayName, smtpUsername, smtpPassword,
                    smtpHost, smtpPort, smtpEnableSsl, timeout);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="emailAddress">发件人邮箱</param>
            /// <param name="displayName">发件人名称</param>
            /// <param name="smtpUsername">帐号</param>
            /// <param name="smtpPassword">密码</param>
            /// <param name="smtpHost">服务主机</param>
            /// <param name="smtpPort">服务端口:默认25,不采用SSL加密,如果采用SSL加密一般为587或者465</param>
            /// <param name="smtpEnableSsl">否SSL加密:默认不加密，如果需要加密则需要对应修改端口号</param>
            /// <param name="timeout">发送超时:秒</param>
            /// <returns></returns>
            public static EmailServer Generate(string emailAddress, string displayName, string smtpUsername,
                string smtpPassword, string smtpHost, int smtpPort = 25, bool smtpEnableSsl = false,
                int timeout = 30)
            {
                if (string.IsNullOrWhiteSpace(emailAddress) || !emailAddress.Contains("@"))
                {
                    return null;
                }

                return new EmailServer()
                {
                    PosterAddress = emailAddress,
                    PosterName = string.IsNullOrWhiteSpace(displayName) ? emailAddress : displayName,
                    Username = smtpUsername,
                    Password = smtpPassword,
                    Host = smtpHost,
                    Port = smtpPort,
                    EnableSsl = smtpEnableSsl,
                    Timeout = timeout
                };
            }
        }

        public class EmailTemplate
        {
            public string Key { get; set; } = "";
            public string Subject { get; set; } = "";
            public string Content { get; set; } = "";
            public bool IsBodyHtml { get; set; } = true;
            public Encoding BodyEncoding { get; set; } = Encoding.UTF8;
            public Encoding SubjectEncoding { get; set; } = Encoding.UTF8;

            public DeliveryNotificationOptions DeliveryNotification { get; set; } =
                DeliveryNotificationOptions.Never;

            public MailPriority Priority { get; set; } = MailPriority.Normal;


            public static ConcurrentDictionary<string, EmailTemplate> Instances { get; set; } =
                new ConcurrentDictionary<string, EmailTemplate>();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="template"></param>
            /// <param name="emailaddress">收件地址</param>
            /// <param name="displayName">收件人名称</param>
            /// <param name="datas"></param>
            /// <returns></returns>
            public static MailMessage Generate(EmailTemplate template, string emailaddress, string displayName,
                Dictionary<string, string> datas)
            {
                if (template != null && emailaddress != null && emailaddress.Contains("@"))
                {
                    var mail = new MailMessage()
                    {
                        Subject = template.Subject,
                        Body = template.Content,
                        BodyEncoding = template.BodyEncoding,
                        SubjectEncoding = template.SubjectEncoding,
                        IsBodyHtml = template.IsBodyHtml,
                        Priority = template.Priority,
                        DeliveryNotificationOptions = template.DeliveryNotification,
                        BodyTransferEncoding = System.Net.Mime.TransferEncoding.Base64
                    };

                    foreach (var item in datas ?? new Dictionary<string, string>())
                    {
                        mail.Body = mail.Body.Replace("{" + item.Key + "}", item.Value);
                    }

                    mail.To.Add(new MailAddress(emailaddress,
                        string.IsNullOrWhiteSpace(displayName) ? emailaddress : displayName));

                    return mail;
                }

                return null;
            }
        }

        // #region Smtp服务器管理配置
        //
        // private static readonly ConcurrentDictionary<string, KeyValuePair<SmtpClient, MailAddress>> _SmtpClients =
        //     new ConcurrentDictionary<string, KeyValuePair<SmtpClient, MailAddress>>();
        //
        // #endregion Smtp服务器管理配置


        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="emailServer"></param>
        public static async void SendMail(this EmailServer emailServer, string mailAddress, string subject,
            string body, bool isBodyHtml = false)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(mailAddress);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isBodyHtml;
            await SendMailAsync(emailServer, mail);
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="emailServer"></param>
        public static async void SendMail(this EmailServer emailServer, MailMessage mail)
        {
            await SendMailAsync(emailServer, mail);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="emailServer"></param>
        public static Task SendMailAsync(this EmailServer emailServer, string mailAddress, string subject,
            string body, bool isBodyHtml = false)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(mailAddress);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isBodyHtml;
            return SendMailAsync(emailServer, mail);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="emailServer"></param>
        public static Task SendMailAsync(this EmailServer emailServer, MailMessage mail)
        {
            if (mail == null)
                throw new ArgumentNullException("mail");
            if (emailServer == null)
            {
                throw new ArgumentNullException("emailServer");
            }

            var sc = new SmtpClient
            {
                TargetName = emailServer.PosterAddress,
                Host = emailServer.Host,
                Port = emailServer.Port,
                EnableSsl = emailServer.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailServer.Username, emailServer.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = emailServer.Timeout * 1000
            };
            mail.From = new MailAddress(emailServer.PosterAddress, emailServer.PosterName);
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
            return sc.SendMailAsync(mail);
        }
    }
}
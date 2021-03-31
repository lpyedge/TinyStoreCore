using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TinyStore.Utils
{
    public static class EmailContext
    {
        static EmailContext()
        {
        }

        public class EmailServer
        {
            public EmailServer()
            {
            }

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
            /// <param name="p_EmailAddress">发邮人</param>
            /// <param name="p_SmtpUsername">帐号</param>
            /// <param name="p_SmtpPassword">密码</param>
            /// <param name="p_SmtpHost">服务主机</param>
            /// <param name="p_SmtpPort">服务端口:默认25,不采用SSL加密,如果采用SSL加密一般为587或者465</param>
            /// <param name="p_SmtpEnableSsl">否SSL加密:默认不加密，如果需要加密则需要对应修改端口号</param>
            /// <param name="p_Timeout">发送超时:秒</param>
            /// <returns></returns>
            public static EmailServer Generate(MailAddress p_EmailAddress, string p_SmtpUsername, string p_SmtpPassword,
                string p_SmtpHost, int p_SmtpPort = 25, bool p_SmtpEnableSsl = false, int p_Timeout = 30)
            {
                return Generate(p_EmailAddress.Address, p_EmailAddress.DisplayName, p_SmtpUsername, p_SmtpPassword,
                    p_SmtpHost, p_SmtpPort, p_SmtpEnableSsl, p_Timeout);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="p_EmailAddress">发件人邮箱</param>
            /// <param name="p_DisplayName">发件人名称</param>
            /// <param name="p_SmtpUsername">帐号</param>
            /// <param name="p_SmtpPassword">密码</param>
            /// <param name="p_SmtpHost">服务主机</param>
            /// <param name="p_SmtpPort">服务端口:默认25,不采用SSL加密,如果采用SSL加密一般为587或者465</param>
            /// <param name="p_SmtpEnableSsl">否SSL加密:默认不加密，如果需要加密则需要对应修改端口号</param>
            /// <param name="p_Timeout">发送超时:秒</param>
            /// <returns></returns>
            public static EmailServer Generate(string p_EmailAddress, string p_DisplayName, string p_SmtpUsername,
                string p_SmtpPassword, string p_SmtpHost, int p_SmtpPort = 25, bool p_SmtpEnableSsl = false,
                int p_Timeout = 30)
            {
                if (string.IsNullOrWhiteSpace(p_EmailAddress) || !p_EmailAddress.Contains("@"))
                {
                    return null;
                }

                return new EmailServer()
                {
                    PosterAddress = p_EmailAddress,
                    PosterName = string.IsNullOrWhiteSpace(p_DisplayName) ? p_EmailAddress : p_DisplayName,
                    Username = p_SmtpUsername,
                    Password = p_SmtpPassword,
                    Host = p_SmtpHost,
                    Port = p_SmtpPort,
                    EnableSsl = p_SmtpEnableSsl,
                    Timeout = p_Timeout
                };
            }
        }

        public class EmailTemplate
        {
            public EmailTemplate()
            {
            }

            public string Key { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
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
                        var name = nameof(item).ToLower();
                        mail.Body = mail.Body.Replace("{" + item.Key + "}", item.Value);
                    }

                    mail.To.Add(new MailAddress(emailaddress,
                        string.IsNullOrWhiteSpace(displayName) ? emailaddress : displayName));

                    return mail;
                }

                return null;
            }
        }

        #region Smtp服务器管理配置

        private static readonly ConcurrentDictionary<string, KeyValuePair<SmtpClient, MailAddress>> _SmtpClients =
            new ConcurrentDictionary<string, KeyValuePair<SmtpClient, MailAddress>>();

        #endregion Smtp服务器管理配置


        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="p_MailAddress"></param>
        /// <param name="p_Subject"></param>
        /// <param name="p_Body"></param>
        /// <param name="p_IsBodyHtml"></param>
        /// <param name="emailServer"></param>
        public static async void SendMail(this EmailServer emailServer, string p_MailAddress, string p_Subject,
            string p_Body, bool p_IsBodyHtml = false)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(p_MailAddress);
            mail.Subject = p_Subject;
            mail.Body = p_Body;
            mail.IsBodyHtml = p_IsBodyHtml;
            await SendMailAsync(emailServer, mail);
        }

        /// <summary>
        /// 同步发送
        /// </summary>
        /// <param name="p_Mail"></param>
        /// <param name="emailServer"></param>
        public static async void SendMail(this EmailServer emailServer, MailMessage p_Mail)
        {
            await SendMailAsync(emailServer, p_Mail);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="p_MailAddress"></param>
        /// <param name="p_Subject"></param>
        /// <param name="p_Body"></param>
        /// <param name="p_IsBodyHtml"></param>
        /// <param name="emailServer"></param>
        public static Task SendMailAsync(this EmailServer emailServer, string p_MailAddress, string p_Subject,
            string p_Body, bool p_IsBodyHtml = false)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(p_MailAddress);
            mail.Subject = p_Subject;
            mail.Body = p_Body;
            mail.IsBodyHtml = p_IsBodyHtml;
            return SendMailAsync(emailServer, mail);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="p_Mail"></param>
        /// <param name="emailServer"></param>
        public static Task SendMailAsync(this EmailServer emailServer, MailMessage p_Mail)
        {
            if (p_Mail == null)
                throw new ArgumentNullException("p_Mail");
            if (emailServer == null)
            {
                throw new ArgumentNullException("emailServer");
            }

            try
            {
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
                p_Mail.From = new MailAddress(emailServer.PosterAddress, emailServer.PosterName);
                ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
                return sc.SendMailAsync(p_Mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LPayments
{
    public abstract class IPayChannel
    {
        protected IPayChannel()
        {
            PayChannnel = GetType().Type2ChannelAttribute();

            Name = this.GetType().FullName;

            Init();
        }

        public EPlatform Platform { get; protected set; }

        public PayChannelAttribute PayChannnel { get; protected set; }


        /// <summary>
        ///     支付接口名称
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        ///     支付接口注释
        /// </summary>
        public virtual string Memo { get; protected set; }

        /// <summary>
        ///     支付接口设置方法
        /// </summary>
        public virtual List<Setting> Settings { get; protected set; }

        protected abstract void Init();

        internal Utils.HttpWebUtility _HWU { get; set; } = new Utils.HttpWebUtility();

        /// <summary>
        ///     支付接口设置方法
        /// </summary>
        public virtual string SettingsJson
        {
            get { return Utils.JsonUtility.Serialize(Settings); }
            set
            {
                foreach (var setting in Settings)
                {
                    setting.Value = "";
                }

                if (!string.IsNullOrWhiteSpace(value))
                    foreach (Setting s in Utils.JsonUtility.Deserialize<List<Setting>>(value))
                    {
                        var setting = Settings.FirstOrDefault(p =>
                            string.Equals(p.Name, s.Name, StringComparison.OrdinalIgnoreCase));
                        if (setting != null)
                            setting.Value = s.Value;
                    }
            }
        }

        /// <summary>
        ///     返回或设置实例Setting的值
        /// </summary>
        /// <param name="settingname">键</param>
        /// <returns>值</returns>
        public string this[string settingname]
        {
            get
            {
                var setting = Settings.FirstOrDefault(p => p.Name == settingname);
                if (setting != null)
                    return setting.Value;
                return "";
            }
            set
            {
                var setting = Settings.FirstOrDefault(p => p.Name == settingname);
                if (setting != null && Regex.IsMatch(value, setting.Regex, RegexOptions.CultureInvariant))
                    setting.Value = value;
            }
        }

        /// <summary>
        ///     支付接口允许接受的货币列表
        /// </summary>
        public List<ECurrency> Currencies { get; protected set; }
    }

    public interface IPay
    {
        /// <summary>
        ///     通知返回方法
        /// </summary>
        /// <returns></returns>
        public virtual PayResult Notify(Microsoft.AspNetCore.Http.HttpContext context)
        {
            PayResult res = null;
            if (context != null)
            {
                var query = new Dictionary<string, string>();
                var head = new Dictionary<string, string>();

                var body = "";
                var form = new Dictionary<string, string>();
                if (context.Request.Method == "POST")
                {
                    try
                    {
                        if (context.Request.ContentLength > 0)
                        {
                            using (var buffer = new MemoryStream())
                            {
                                context.Request.Body.CopyTo(buffer);
                                body = Encoding.UTF8.GetString(buffer.ToArray());
                            }
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (context.Request.Form?.Count > 0)
                        {
                            foreach (var key in context.Request.Form.Keys)
                                form[key] = context.Request.Form[key];
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (var key in context.Request.Query.Keys)
                    query[key] = context.Request.Query[key];
                foreach (var key in context.Request.Headers.Keys)
                    head[key] = context.Request.Headers[key];
                res = Notify(form, query, head, body, context.Connection.RemoteIpAddress.ToString());
            }

            return res;
        }

        /// <summary>
        ///     方法
        /// </summary>
        /// <returns></returns>
        PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip);

        /// <summary>
        ///     生成支付代码
        /// </summary>
        /// <param name="p_OrderId">订单号码</param>
        /// <param name="p_Amount">订单金额</param>
        /// <param name="p_Currency">货币种类</param>
        /// <param name="p_OrderName">订单名称</param>
        /// <param name="p_ClientIP">客户IP</param>
        /// <param name="p_ReturnUrl">返回地址</param>
        /// <param name="p_NotifyUrl">通知地址</param>
        /// <param name="p_CancelUrl">取消地址</param>
        /// <param name="extend_params">额外参数</param>
        /// <returns></returns>
        PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "",
            string p_CancelUrl = "", dynamic extend_params = null);
    }

    /// <summary>
    /// 退款
    /// </summary>
    public interface IRefund
    {
        bool Refund(string p_OrderId, string p_TxnId);
    }
   
    /// <summary>
    /// 转账
    /// </summary>
    public interface ITransfer
    {
        /// <summary>
        /// 转账
        /// </summary>
        dynamic Transfer(dynamic extend);
    }
}
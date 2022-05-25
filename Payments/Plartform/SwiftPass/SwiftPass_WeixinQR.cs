// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Net;
// using System.Text;
// using System.Web;
// using System.Xml;
//
// namespace Payments.Plartform.SwiftPass
// {
//     [PayChannel("威富通", EChannel.Wechat, ePayType = EPayType.QRcode)]
//     public class WeixinQR : IPayChannel
//     {
//         //wxa173f208a85f859b
//         //5627da0ed6d72e23f0bae320a3827e69
//
//         public const string MchId = "MchId";
//         public const string SecretKey = "SecretKey";
//
//         public WeixinQR()
//         {
//             Init();
//         }
//
//         public WeixinQR(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MchId, Description = "微信支付的商家帐号MchId", Regex = @"^[\w\.@]+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "微信支付的Key", Regex = @"^\w+$", Requied = true}
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.CNY
//             };
//         }
//
//         public override PayResult Notify(Microsoft.AspNetCore.Http.HttpContext context)
//         {
//             return base.Notify(context);
//         }
//
//         public override PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//             IDictionary<string, string> head, string body, string notifyip)
//         {
//             PayResult result = new PayResult
//             {
//                 Status = PayResult.EStatus.Pending,
//                 Message = "failed"
//             };
//
//             var resHandler = new ClientResponseHandler();
//             resHandler.setContent(body);
//             resHandler.setKey(this[SecretKey]);
//
//             var resParam = resHandler.getAllParameters();
//             if (resHandler.isTenpaySign())
//                 if (int.Parse(resParam["status"].ToString()) == 0
//                     && int.Parse(resParam["result_code"].ToString()) == 0
//                     && resParam["mch_id"].ToString() == this[MchId])
//                 {
//                     //Utils.writeFile("接口回调", resParam); //通知返回参数写入result.txt文本文件。
//                     //此处可以在添加相关处理业务，校验通知参数中的商户订单号out_trade_no和金额total_fee是否和商户业务系统的单号和金额是否一致，一致后方可更新数据库表中的记录。
//
//                     result = new PayResult
//                     {
//                         Status = PayResult.EStatus.Completed,
//                         OrderName = "",
//                         OrderID = resParam["out_trade_no"].ToString(),
//                         Amount = double.Parse(resParam["total_fee"].ToString()) / 100,
//                         Tax = -1,
//                         Currency = Utils.Core.Parse<ECurrency>(resParam["fee_type"].ToString()),
//                         Business = this[MchId],
//                         TxnID = resParam["transaction_id"].ToString(),
//                         PaymentName = Name + "_" + resParam["bank_type"],
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "success",
//                     };
//                 }
//                 else
//                 {
//                     result.Message = "failure1";
//                 }
//             else
//                 result.Message = "failure2";
//
//             return result;
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//
//             if (string.IsNullOrEmpty(this[MchId])) throw new ArgumentNullException("MchId)");
//             if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey)");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var pt = new PayTicket();
//
//             var reqHandler = new RequestHandler(null);
//             //初始化数据
//             reqHandler.setGateUrl("https://pay.swiftpass.cn/pay/gateway");
//             reqHandler.setKey(this[SecretKey]);
//             reqHandler.setParameter("out_trade_no", p_OrderId); //商户订单号
//             reqHandler.setParameter("body", p_OrderName); //商品描述
//             reqHandler.setParameter("attach", p_OrderName); //附加信息
//             reqHandler.setParameter("total_fee", (p_Amount * 100).ToString("0.##")); //总金额
//             reqHandler.setParameter("mch_create_ip", p_ClientIP.ToString()); //终端IP
//             reqHandler.setParameter("time_start", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss")); //订单生成时间
//             reqHandler.setParameter("time_expire",
//                 DateTime.UtcNow.AddHours(8).AddMinutes(30).ToString("yyyyMMddHHmmss")); //订单超时时间
//             reqHandler.setParameter("service", "pay.weixin.native"); //接口类型：pay.weixin.native
//             reqHandler.setParameter("mch_id", this[MchId]); //必填项，商户号，由威富通分配
//             reqHandler.setParameter("version", "2.0"); //接口版本号
//             reqHandler.setParameter("notify_url", p_NotifyUrl);
//             //通知地址，必填项，接收威富通通知的URL，需给绝对路径，255字符内;此URL要保证外网能访问
//             reqHandler.setParameter("nonce_str", Guid.NewGuid().ToString("N")); //随机字符串，必填项，不长于 32 位
//             reqHandler.createSign(); //创建签名
//             //以上参数进行签名
//             var data = reqHandler.getAllParametersXml(); //生成XML报文
//             var reqContent = new Dictionary<string, string>();
//             reqContent.Add("url", reqHandler.getGateUrl());
//             reqContent.Add("data", data);
//
//             var pay = new PayHttpClient();
//             pay.setReqContent(reqContent);
//
//             if (pay.call())
//             {
//                 var resHandler = new ClientResponseHandler();
//                 resHandler.setContent(pay.getResContent());
//                 resHandler.setKey(this[SecretKey]);
//                 var param = resHandler.getAllParameters();
//                 if (resHandler.isTenpaySign())
//                     if (int.Parse(param["status"].ToString()) == 0 && int.Parse(param["result_code"].ToString()) == 0)
//                     {
//                         pt.Url = param["code_url"].ToString();
//                         //string imgbase64 = Core.QR(pt.Url, Core.WXIconBase64);
//                         //pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
//                         //pt.Extra = imgbase64;
//                     }
//                     else
//                         pt.Message = "'错误代码：" + param["err_code"] + ",错误信息：" + param["err_msg"];
//                 else
//                     pt.Message = "'错误代码：" + param["status"] + ",错误信息：" + param["message"];
//             }
//             else
//             {
//                 pt.Message = "'错误代码：" + pay.getResponseCode() + ",错误信息：" + pay.getErrInfo();
//             }
//
//             return pt;
//         }
//
//         #region 微信支付辅助类
//
//         internal class RequestHandler
//         {
//             /// <summary>
//             ///     debug信息
//             /// </summary>
//             private string debugInfo;
//
//             /// <summary>
//             ///     网关url地址
//             /// </summary>
//             private string gateUrl;
//
//             protected Microsoft.AspNetCore.Http.HttpContext httpContext;
//
//             /// <summary>
//             ///     密钥
//             /// </summary>
//             private string key;
//
//             /// <summary>
//             ///     请求的参数
//             /// </summary>
//             protected Hashtable parameters;
//
//             public RequestHandler(Microsoft.AspNetCore.Http.HttpContext httpContext)
//             {
//                 parameters = new Hashtable();
//
//                 this.httpContext = httpContext;
//             }
//
//             /// <summary>
//             ///     初始化函数
//             /// </summary>
//             public virtual void init()
//             {
//                 //nothing to do
//             }
//
//             /// <summary>
//             ///     获取入口地址,不包含参数值
//             /// </summary>
//             /// <returns></returns>
//             public string getGateUrl()
//             {
//                 return gateUrl;
//             }
//
//             /// <summary>
//             ///     设置入口地址,不包含参数值
//             /// </summary>
//             /// <param name="gateUrl">入口地址</param>
//             public void setGateUrl(string gateUrl)
//             {
//                 this.gateUrl = gateUrl;
//             }
//
//             /// <summary>
//             ///     获取密钥
//             /// </summary>
//             /// <returns></returns>
//             public string getKey()
//             {
//                 return key;
//             }
//
//             /// <summary>
//             ///     设置密钥
//             /// </summary>
//             /// <param name="key">密钥字符串</param>
//             public void setKey(string key)
//             {
//                 this.key = key;
//             }
//
//             /// <summary>
//             ///     获取带参数的请求URL
//             /// </summary>
//             /// <returns></returns>
//             public virtual string getRequestURL()
//             {
//                 createSign();
//
//                 var sb = new StringBuilder();
//                 var akeys = new ArrayList(parameters.Keys);
//                 akeys.Sort();
//                 foreach (string k in akeys)
//                 {
//                     var v = (string) parameters[k];
//                     if (null != v && "key".CompareTo(k) != 0)
//                         sb.Append(k + "=" + HttpUtility.UrlEncode(v.Trim()) + "&");
//                 }
//
//                 //去掉最后一个&
//                 if (sb.Length > 0)
//                     sb.Remove(sb.Length - 1, 1);
//
//                 return getGateUrl() + "?" + sb;
//             }
//
//             /// <summary>
//             ///     创建md5摘要,规则是:按参数名称a-z排序,遇到空值的参数不参加签名。
//             /// </summary>
//             public virtual void createSign()
//             {
//                 var sb = new StringBuilder();
//
//                 var akeys = new ArrayList(parameters.Keys);
//                 akeys.Sort();
//
//                 foreach (string k in akeys)
//                 {
//                     var v = (string) parameters[k];
//                     if (null != v && "".CompareTo(v) != 0
//                                   && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
//                         sb.Append(k + "=" + v + "&");
//                 }
//
//                 sb.Append("key=" + getKey());
//
//                 var sign = Utils.Core.MD5(sb.ToString()).ToUpper();
//
//                 setParameter("sign", sign);
//
//                 //debug信息
//                 setDebugInfo(sb + " => sign:" + sign);
//             }
//
//             /// <summary>
//             ///     获取参数值
//             /// </summary>
//             /// <param name="parameter">参数名</param>
//             /// <returns></returns>
//             public string getParameter(string parameter)
//             {
//                 var s = (string) parameters[parameter];
//                 return null == s ? "" : s;
//             }
//
//             /// <summary>
//             ///     设置参数值
//             /// </summary>
//             /// <param name="parameter">参数名</param>
//             /// <param name="parameterValue">参数值</param>
//             public void setParameter(string parameter, string parameterValue)
//             {
//                 if (parameter != null && parameter != "")
//                 {
//                     if (parameters.Contains(parameter))
//                         parameters.Remove(parameter);
//
//                     parameters.Add(parameter, parameterValue);
//                 }
//             }
//
//             public void doSend()
//             {
//                 httpContext.Response.Redirect(getRequestURL());
//             }
//
//             /// <summary>
//             ///     获取debug信息
//             /// </summary>
//             /// <returns></returns>
//             public string getDebugInfo()
//             {
//                 return debugInfo;
//             }
//
//             /// <summary>
//             ///     设置debug信息
//             /// </summary>
//             /// <param name="debugInfo"></param>
//             public void setDebugInfo(string debugInfo)
//             {
//                 this.debugInfo = debugInfo;
//             }
//
//             /// <summary>
//             ///     获取所有参数
//             /// </summary>
//             /// <returns></returns>
//             public Hashtable getAllParameters()
//             {
//                 return parameters;
//             }
//
//             public string getAllParametersXml()
//             {
//                 var sb = new StringBuilder("<xml>");
//                 foreach (DictionaryEntry de in parameters)
//                 {
//                     var key = de.Key.ToString();
//                     sb.Append("<")
//                         .Append(key)
//                         .Append("><![CDATA[")
//                         .Append(de.Value)
//                         .Append("]]></")
//                         .Append(key)
//                         .Append(">");
//                 }
//
//                 return sb.Append("</xml>").ToString();
//             }
//
//             /// <summary>
//             ///     获取编码
//             /// </summary>
//             /// <returns></returns>
//             protected virtual string getCharset()
//             {
//                 //return this.httpContext.Request.ContentEncoding.BodyName;
//                 return "utf-8";
//             }
//
//             /// <summary>
//             ///     设置页面提交的请求参数
//             /// </summary>
//             /// <param name="paramNames">参数名</param>
//             public void setReqParameters(string[] paramNames)
//             {
//                 parameters.Clear();
//                 foreach (var pName in paramNames)
//                 {
//                     var reqVal = httpContext.Request.Query[pName];
//                     if (string.IsNullOrEmpty(reqVal))
//                         continue;
//                     parameters.Add(pName, reqVal);
//                 }
//             }
//         }
//
//         internal class PayHttpClient
//         {
//             /// <summary>
//             ///     错误信息
//             /// </summary>
//             private string errInfo;
//
//             /// <summary>
//             ///     请求方法
//             /// </summary>
//             private string method;
//
//             /// <summary>
//             ///     请求内容
//             /// </summary>
//             private Dictionary<string, string> reqContent;
//
//             /// <summary>
//             ///     应答内容
//             /// </summary>
//             private string resContent;
//
//             /// <summary>
//             ///     http应答编码
//             /// </summary>
//             private int responseCode;
//
//             /// <summary>
//             ///     超时时间,以秒为单位
//             /// </summary>
//             private int timeOut;
//
//             public PayHttpClient()
//             {
//                 reqContent = new Dictionary<string, string>();
//                 reqContent["url"] = "";
//                 reqContent["data"] = "";
//
//                 resContent = "";
//                 method = "POST";
//                 errInfo = "";
//                 timeOut = 1 * 60; //5分钟
//
//                 responseCode = 0;
//             }
//
//             /// <summary>
//             ///     设置请求内容
//             /// </summary>
//             /// <param name="reqContent">内容</param>
//             public void setReqContent(Dictionary<string, string> reqContent)
//             {
//                 this.reqContent = reqContent;
//             }
//
//             /// <summary>
//             ///     获取结果内容
//             /// </summary>
//             /// <returns></returns>
//             public string getResContent()
//             {
//                 return resContent;
//             }
//
//             /// <summary>
//             ///     设置请求方法
//             /// </summary>
//             /// <param name="method">请求方法，可选:POST或GET</param>
//             public void setMethod(string method)
//             {
//                 this.method = method;
//             }
//
//             /// <summary>
//             ///     获取错误信息
//             /// </summary>
//             /// <returns></returns>
//             public string getErrInfo()
//             {
//                 return errInfo;
//             }
//
//             /// <summary>
//             ///     设置超时时间
//             /// </summary>
//             /// <param name="timeOut">超时时间，单位：秒</param>
//             public void setTimeOut(int timeOut)
//             {
//                 this.timeOut = timeOut;
//             }
//
//             /// <summary>
//             ///     获取http状态码
//             /// </summary>
//             /// <returns></returns>
//             public int getResponseCode()
//             {
//                 return responseCode;
//             }
//
//             //执行http调用
//             public bool call()
//             {
//                 StreamReader sr = null;
//                 HttpWebResponse wr = null;
//
//                 HttpWebRequest hp = null;
//                 try
//                 {
//                     hp = (HttpWebRequest) WebRequest.Create(reqContent["url"]);
//                     var postData = reqContent["data"];
//
//                     hp.Timeout = timeOut * 1000;
//                     //hp.Timeout = 10 * 1000;
//                     if (postData != null)
//                     {
//                         var data = Encoding.UTF8.GetBytes(postData);
//                         hp.Method = "POST";
//
//                         hp.ContentLength = data.Length;
//
//                         var ws = hp.GetRequestStream();
//
//                         // 发送数据
//                         ws.Write(data, 0, data.Length);
//                         ws.Close();
//                     }
//
//                     wr = (HttpWebResponse) hp.GetResponse();
//                     sr = new StreamReader(wr.GetResponseStream(), Encoding.UTF8);
//
//                     resContent = sr.ReadToEnd();
//                     sr.Close();
//                     wr.Close();
//                 }
//                 catch (Exception exp)
//                 {
//                     errInfo += exp.Message;
//                     if (wr != null)
//                         responseCode = Convert.ToInt32(wr.StatusCode);
//
//                     return false;
//                 }
//
//                 responseCode = Convert.ToInt32(wr.StatusCode);
//
//                 return true;
//             }
//         }
//
//         /// <summary>
//         ///     客户端消息返回头
//         /// </summary>
//         internal class ClientResponseHandler
//         {
//             private string charset = "UTF-8";
//
//             /// <summary>
//             ///     原始内容
//             /// </summary>
//             protected string content;
//
//             /// <summary>
//             ///     debug信息
//             /// </summary>
//             private string debugInfo;
//
//             /// <summary>
//             ///     密钥
//             /// </summary>
//             private string key;
//
//             /// <summary>
//             ///     应答的参数
//             /// </summary>
//             protected Hashtable parameters;
//
//             /// <summary>
//             ///     获取服务器通知数据方式，进行参数获取
//             /// </summary>
//             public ClientResponseHandler()
//             {
//                 parameters = new Hashtable();
//             }
//
//             /// <summary>
//             ///     获取返回内容
//             /// </summary>
//             /// <returns></returns>
//             public string getContent()
//             {
//                 return content;
//             }
//
//             /// <summary>
//             ///     设置返回内容
//             /// </summary>
//             /// <param name="content">XML内容</param>
//             public virtual void setContent(string content)
//             {
//                 this.content = content;
//                 var xmlDoc = new XmlDocument();
//                 xmlDoc.LoadXml(content);
//                 var root = xmlDoc.SelectSingleNode("xml");
//                 var xnl = root.ChildNodes;
//
//                 foreach (XmlNode xnf in xnl)
//                     setParameter(xnf.Name, xnf.InnerText);
//             }
//
//             /// <summary>
//             ///     获取密钥
//             /// </summary>
//             /// <returns></returns>
//             public string getKey()
//             {
//                 return key;
//             }
//
//             /// <summary>
//             ///     设置密钥
//             /// </summary>
//             /// <param name="key">密钥</param>
//             public void setKey(string key)
//             {
//                 this.key = key;
//             }
//
//             /// <summary>
//             ///     获取参数值
//             /// </summary>
//             /// <param name="parameter">参数名</param>
//             /// <returns></returns>
//             public string getParameter(string parameter)
//             {
//                 var s = (string) parameters[parameter];
//                 return null == s ? "" : s;
//             }
//
//             /// <summary>
//             ///     设置参数值
//             /// </summary>
//             /// <param name="parameter">参数名</param>
//             /// <param name="parameterValue">参数值</param>
//             public void setParameter(string parameter, string parameterValue)
//             {
//                 if (parameter != null && parameter != "")
//                 {
//                     if (parameters.Contains(parameter))
//                         parameters.Remove(parameter);
//
//                     parameters.Add(parameter, parameterValue);
//                 }
//             }
//
//             /// <summary>
//             ///     是否威富通签名,规则是:按参数名称a-z排序,遇到空值的参数不参加签名。
//             /// </summary>
//             /// <returns></returns>
//             public virtual bool isTenpaySign()
//             {
//                 var sb = new StringBuilder();
//
//                 var akeys = new ArrayList(parameters.Keys);
//                 akeys.Sort();
//
//                 foreach (string k in akeys)
//                 {
//                     var v = (string) parameters[k];
//                     if (null != v && "".CompareTo(v) != 0
//                                   && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
//                         sb.Append(k + "=" + v + "&");
//                 }
//
//                 sb.Append("key=" + getKey());
//                 var sign = Utils.Core.MD5(sb.ToString()).ToLower();
//
//                 //debug信息
//                 setDebugInfo(sb + " => sign:" + sign);
//                 return getParameter("sign").ToLower().Equals(sign);
//             }
//
//             /// <summary>
//             ///     获取debug信息
//             /// </summary>
//             /// <returns></returns>
//             public string getDebugInfo()
//             {
//                 return debugInfo;
//             }
//
//             /// <summary>
//             ///     设置debug信息
//             /// </summary>
//             /// <param name="debugInfo"></param>
//             protected void setDebugInfo(string debugInfo)
//             {
//                 this.debugInfo = debugInfo;
//             }
//
//             /// <summary>
//             ///     获取编码
//             /// </summary>
//             /// <returns></returns>
//             protected virtual string getCharset()
//             {
//                 return charset;
//             }
//
//             /// <summary>
//             ///     设置编码
//             /// </summary>
//             /// <param name="charset">编码</param>
//             public void setCharset(string charset)
//             {
//                 this.charset = charset;
//             }
//
//             /// <summary>
//             ///     是否威富通签名,规则是:按参数名称a-z排序,遇到空值的参数不参加签名。
//             /// </summary>
//             /// <param name="akeys"></param>
//             /// <returns></returns>
//             public virtual bool _isTenpaySign(ArrayList akeys)
//             {
//                 var sb = new StringBuilder();
//
//                 foreach (string k in akeys)
//                 {
//                     var v = (string) parameters[k];
//                     if (null != v && "".CompareTo(v) != 0
//                                   && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
//                         sb.Append(k + "=" + v + "&");
//                 }
//
//                 sb.Append("key=" + getKey());
//                 var sign = Utils.Core.MD5(sb.ToString()).ToLower();
//
//                 //debug信息
//                 setDebugInfo(sb + " => sign:" + sign);
//                 return getParameter("sign").ToLower().Equals(sign);
//             }
//
//             /// <summary>
//             ///     获取返回的所有参数
//             /// </summary>
//             /// <returns></returns>
//             public Hashtable getAllParameters()
//             {
//                 return parameters;
//             }
//         }
//
//         #endregion 微信支付辅助类
//     }
// }
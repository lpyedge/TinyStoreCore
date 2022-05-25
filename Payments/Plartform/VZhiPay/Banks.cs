// using System;
// using System.Collections.Generic;
// using System.Net;
//
// namespace Payments.Plartform.VZhiPay
// {
//     [PayChannel("惊鸿云", EChannel.ChinaBanks, ePayType = EPayType.PC)]
//     public class Banks : _VZhiPay
//     {
//         public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
//         {
//             [EChinaBank.SPDB] = "9020",
//             [EChinaBank.ICBC] = "9040",
//             [EChinaBank.BOC] = "9021",
//             [EChinaBank.CCB] = "9008",
//             [EChinaBank.CIB] = "9015",
//             [EChinaBank.CEB] = "9002",
//             [EChinaBank.BCOM] = "9005",
//             [EChinaBank.CMB] = "9004",
//             [EChinaBank.CMBC] = "9010",
//             [EChinaBank.CITIC] = "9001",
//             [EChinaBank.HXB] = "9003",
//             //9079	东莞银行
//             [EChinaBank.ABC] = "9009",
//             [EChinaBank.GZCB] = "9016",
//             //9045	南粤银行
//             //9127	包商银行
//             [EChinaBank.PSBC] = "9006",
//             [EChinaBank.GDB] = "9012",
//             [EChinaBank.SZPAB] = "9013",
//             [EChinaBank.BOBJ] = "9014",
//             [EChinaBank.BOSH] = "9126",
//             [EChinaBank.JSB] = "9017",
//             [EChinaBank.NJCB] = "9018",
//             [EChinaBank.HZCB] = "9019",
//             [EChinaBank.BRCB] = "9028",
//             //9011	重庆农村商业银行
//             //9022    东莞农村商业银行
//             [EChinaBank.HSB] = "9023",
//             [EChinaBank.NBCB] = "9024",
//             [EChinaBank.BOCD] = "9025",
//             //9026    厦门银行
//             //9027    汉口银行
//             //9029    九江银行
//             //9030    晋中银行
//             //9031    江苏长江商业银行
//             //9032    洛阳银行
//             //9033    保定银行
//             //9034    齐商银行
//             //9035    潍坊银行
//             //9036    莱商银行
//             //9037    长安银行
//             //9038    德州银行
//             //9039    日照银行
//             //9000    泰安市商业银行
//             //9041    临商银行
//             //9042    东营莱商村镇银行
//             //9043    东营银行
//             //9044    威海市商业银行
//             //9128    枣庄银行
//             //9046    济宁银行
//             //9047    烟台银行
//             //9048    湖北省农村信用社联合社
//             //9049    甘肃省农村信用社联合社
//             //9050    天津滨海农村商业银行
//             //9051    河北省农村信用社联合社
//             //9052    陕西省农村信用社联合社
//             //9053    武汉农村商业银行
//             //9054    内蒙古自治区农村信用社联合社
//             //9055    江苏省农村信用社联合社
//             //9056    河南省农村信用社联合社
//             //9057    山西省农村信用社联合社
//             //9058    吉林省农村信用社联合社
//             //9059    四川省农村信用社联合社
//             //9060    江西银行
//             //9061    龙江银行
//             //9062    鄂尔多斯银行
//             //9064    攀枝花市商业银行
//             //9065    恒丰银行
//             //9066    贵阳银行
//             //9067    锦州银行
//             //9068    渤海银行
//             //9069    兰州银行
//             //9070    浙江泰隆商业银行
//             //9071    桂林银行
//             //9072    新韩银行
//             //9073    营口银行
//             //9074    辽阳银行
//             //9075    西安银行
//             //9076    张家口市商业银行
//             //9077    青岛银行
//             //9078    哈尔滨银行
//             //9129    湖南省农村信用社联合社
//             //9080    江西省农村信用社联合社
//             //9081    辽宁省农村信用社联合社
//             //9082    广西壮族自治区农村信用社联合社
//             //9083    新疆维吾尔自治区农村信用社联合社
//             //9084    黄河农村商业银行
//             //9085    天津农村商业银行
//             //9086    盘锦市商业银行
//             //9087    湖北银行
//             //9088    盛京银行
//             //9089    重庆银行
//             //9125    海南银行
//             //9091    大连银行
//             //9092    广州农商银行
//             //9093    浙商银行
//             //9094    郑州银行
//             //9095    鄞州农村合作银行
//             //9096    遂宁市商业银行
//             //9097    浙江稠州商业银行
//             //9098    云南省农村信用社联合社
//             //9099    大连开发区鑫汇村镇银行
//             //9100    长治银行
//             //9101    泉州银行
//             //9102    铁岭银行
//             //9103    宁夏银行
//             //9104    晋城银行
//             //9105    吉林银行
//             //9106    天津武清村镇银行
//             //9107    阜新银行
//             //9108    秦皇岛银行
//             //9109    江南农村商业银行
//             //9110    太仓农村商业银行
//             //9111    平顶山银行
//             //9112    常熟农村商业银行
//             //9113    上海农商银行
//             //9114    兰州农村商业银行
//             //9115    联合村镇银行
//             //9116    厦门国际银行
//             //9117    营口沿海银行
//             //9118    海南省农村信用社联合社
//             //9119    天津武清村镇银行
//             //9120    中原银行
//             //9121    华融湘江银行
//             //9122    泸州市商业银行
//             //9123    贵州省农村信用社联合社
//             //9124    成都农商银行
//             //9130    国家开发银行
//             //9131    中国进出口银行
//             //9132    中国农业发展银行
//             //9133    天津银行股份有限公司
//             //9134    廊坊银行股份有限公司
//             //9135    温州银行股份有限公司
//             //9136    台州银行股份有限公司
//             //9137    齐鲁银行
//             //9138    珠海华润银行
//             //9139    乌鲁木齐市商业银行
//             //9140    无锡农村商业银行
//             //9141    江苏江阴农村商业银行
//             //9142    昆山农村商业银行
//             //9143    吴江农村商业银行
//             //9144    张家港农村商业银行
//             //9145    佛山顺德农村商业银行
//             //9146    深圳龙岗鼎业村镇银行
//             //9147    深圳福田银座村镇银行
//             //9148    深圳南山宝生村镇银行
//             //9149    深圳龙岗国安村镇银行
//             //9150    黑龙江省农村信用社联合社
//             //9151    浙江省农村信用社联合社
//             //9152    安徽省农村信用联社
//             //9153    福建省农村信用社联合社
//             //9154    山东省农村信用社联合社
//             //9155    广东省农村信用社联合社
//             //9156    青海省农村信用社联合社
//             [EChinaBank.TCCB] = "9133",
//         };
//
//         public Banks() : base()
//         {
//             m_channel = "gateway";
//             m_qrcode = false;
//             m_scenesType = "PC";
//         }
//
//         public Banks(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_channel = "gateway";
//             m_qrcode = false;
//             m_scenesType = "PC";
//         }
//
//         public class PayExtend
//         {
//             public EChinaBank Bank { get; set; }
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (extend_params == null) throw new ArgumentNullException("extend_params");
//             try
//             {
//                 var extend = Utils.JsonUtility.Deserialize<PayExtend>(Utils.JsonUtility.Serialize(extend_params));
//                 m_bankcode = BankDic[extend.Bank];
//             }
//             catch (Exception ex)
//             {
//                 throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
//             }
//             return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
//         }
//     }
//     [PayChannel("惊鸿云", EChannel.ChinaBanks, ePayType = EPayType.H5)]
//     public class Banks_Wap : Banks
//     {
//         public Banks_Wap() : base()
//         {
//             m_channel = "gateway";
//             m_qrcode = false;
//             m_scenesType = "WAP";
//         }
//
//         public Banks_Wap(string p_SettingsJson) : base(p_SettingsJson)
//         {
//             m_channel = "gateway";
//             m_qrcode = false;
//             m_scenesType = "WAP";
//         }
//     }
// }
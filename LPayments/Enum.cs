using System.ComponentModel;

namespace LPayments
{
    public enum EPlatform:int
    {
        /// <summary>
        /// Paypal
        /// </summary>
        [Platform("Paypal", "贝宝", SiteUrl = "https://www.paypal.com", NotifyProxy = true)]
        Paypal=1,
        /// <summary>
        /// Skrill
        /// </summary>
        [Platform("Skrill", "原moneybooks支付", SiteUrl = "https://www.skrill.com")]
        Skrill=2,
        /// <summary>
        /// 蚂蚁金服
        /// </summary>
        [PlatformAttribute("蚂蚁金服", "支付宝开放平台", SiteUrl = "https://openhome.alipay.com/docCenter/docCenter.htm", NotifyProxy = true)]
        Alipay=5,
        /// <summary>
        /// 支付宝旧版
        /// </summary>
        [PlatformAttribute("支付宝旧版", "", SiteUrl = "https://www.alipay.com",NotifyProxy = true)]
        AlipayO =6,
        /// <summary>
        /// 微信支付
        /// </summary>
        [Platform("微信支付", "", SiteUrl = "https://pay.weixin.qq.com")]
        WeChat =7,
        /// <summary>
        /// UnionPay
        /// </summary>
        [Platform("UnionPay", "银联钱包", SiteUrl = "https://www.unionpay.com")]
        UnionPay=8,
        
        
        /// <summary>
        /// 铠世宝
        /// </summary>
        [Platform("铠世宝", "CashRun瑞士支付平台", SiteUrl = "http://www.cashrun.com")]
        CashRun =20,
        /// <summary>
        /// 全球数码
        /// </summary>
        [Platform("全球数码", "G2A.COM", SiteUrl = "https://pay.g2a.com")]
        G2A =21,
        /// <summary>
        /// 支付墙
        /// </summary>
        [Platform("支付墙(强)", "Paymentwall一站式支付平台", SiteUrl = "https://www.paymentwall.com/")]
        PaymentWall = 22,
        /// <summary>
        /// 快钱99Bill
        /// </summary>
        [Platform("快钱99Bill", "贝宝", SiteUrl = "https://www.99bill.com")]
        KuaiQian=23,
        /// <summary>
        /// Xfers
        /// </summary>
        [Platform("Xfers", "新加坡支付平台", SiteUrl = "https://www.xfers.com")]
        Xfers=24,
        /// <summary>
        /// WebMoney
        /// </summary>
        [Platform("WebMoney", "", SiteUrl = "https://www.wmtransfer.com")]
        WebMoney=25,
        
        /// <summary>
        /// SofortBank
        /// </summary>
        [Platform("SofortBank", "德国银行支付平台", SiteUrl = "http://www.sofortbank.be")]
        SofortBank=26,
        
        /// <summary>
        /// Smart2Pay
        /// </summary>
        [Platform("Smart2Pay", "", SiteUrl = "https://smart2pay.com")]
        Smart2Pay=27,
        
        /// <summary>
        /// Paynet
        /// </summary>
        [Platform("Paynet", "法国支付平台", SiteUrl = "https://www.payment.net")]
        Paynet=28,
        
        /// <summary>
        /// Payssion
        /// </summary>
        [Platform("Payssion","",SiteUrl = "https://payssion.com")]
        Payssion=29,
        
        /// <summary>
        /// GPayTr
        /// </summary>
        [Platform("GPayTr", "土耳其信用卡收款", SiteUrl = "https://gpay.com.tr/")]
        GPayTr=50,
        /// <summary>
        /// 聚合支付
        /// </summary>
        [Platform("聚合支付", "", SiteUrl = "http://www.ijuhepay.com")]
        iJuHePay=51,
        /// <summary>
        /// 启赟
        /// </summary>
        [Platform("IPayLinks", "", SiteUrl = "https://www.ipaylinks.com")]
        IPayLinks=52,
        /// <summary>
        /// 橙e付
        /// </summary>
        [Platform("橙e付", "平安银行旗下支付平台", SiteUrl = "http://www.orangebank.com.cn")]
        OrangeBank=53,
        
        /// <summary>
        /// 盛付通
        /// </summary>
        [Platform("盛付通", "", SiteUrl = "https://www.shengpay.com")]
        ShengPay=54,
    }

    /// <summary>
    /// 支付通道
    /// </summary>
    public enum EChannel
    {
        [Description("支付宝")] AliPay,

        [Description("百度钱包")] BaiduPay,
        [Description("京东支付")] JdPay,

        [Description("支付宝批量付款")] AliPayBatch,

        [Description("微信")] WeChat,
        [Description("信用卡")] CreditCard,
        [Description("借记卡")] DebitCard,
        [Description("Palpal贝宝")] Paypal,
        [Description("PaypalExpress贝宝快速结账")] PaypalExpress,
        [Description("PaypalSubscribe贝宝订阅支付")] PaypalSubscribe,
        [Description("财付通")] TenPay,
        [Description("比特币")] Bitcoin,
        [Description("SofortBank德国网银在线")] SofortBank,
        [Description("Paysafecard欧洲预付费卡")] PaySafeCard,
        [Description("Webmoney电子钱包")] WebMoney,
        [Description("HasPay有付")] HasPay,
        [Description("PaymentWall支付强")] PaymentWall,
        [Description("Payssion聚合支付")] Payssion,
        [Description("Smart2Pay信用卡网关")] Smart2Pay,
        [Description("UnionPay银联在线")] UnionPay,
        [Description("Xfers新加坡")] Xfers,
        [Description("XfersExpress新加坡")] XfersExpress,
        [Description("G2APay全球数码聚合支付")] G2APay,
        [Description("MINTprepaid现金预付费卡")] MINT,
        [Description("Bancodobrasil巴西")] Bancodobrasil,
        [Description("Boleto巴西")] Boleto,
        [Description("Bradesco巴西")] Bradesco,
        [Description("Caixa巴西")] Caixa,
        [Description("Cashu中东")] Cashu,
        [Description("Dineromail阿根廷")] Dineromail,
        [Description("Enets新加坡")] Enets,
        [Description("Hsbc巴西")] Hsbc,
        [Description("Itau巴西")] Itau,
        [Description("MOLPoints东南亚")] MOLPoints,
        [Description("MOLPay东南亚")] MOLPay,
        [Description("Neosurf欧洲")] Neosurf,
        [Description("Onecard中东")] Onecard,
        [Description("Openbucks美国")] Openbucks,
        [Description("Poli澳大利亚")] Poli,
        [Description("QiWi俄罗斯")] QiWi,
        [Description("Santander巴西")] Santander,
        [Description("TrustPay")] TrustPay,
        [Description("Yandex俄罗斯")] Yandex,

        [Description("Multibanco葡萄牙")] Multibanco,
        [Description("iDeal荷兰")] iDeal,
        [Description("P24波兰")] P24,
        [Description("Payu波兰")] Payu,
        [Description("Bancontact比利时")] Bancontact,
        [Description("EPS奥地利")] EPS,

        [Description("Giropay德国")] Giropay,
        [Description("Dotpay波兰")] Dotpay,
        [Description("欧洲银行汇款")] EuropeBankTransfer,

        [Description("国内银行网关")] ChinaBanks,
        [Description("国内银行网关")] ChinaBank,
        [Description("国内银行网关(信用卡)")] ChinaBankCredit,
        [Description("国内银行网关(借记卡)")] ChinaBankDebit,
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum EPayType
    {
        [Description("二维码")] QRcode = 1,
        [Description("电脑网关")] PC = 2,
        [Description("手机网关")] H5 = 3, 
        [Description("App")] App = 4,
        [Description("Other")] Other =5
    }

    public enum EAction
    {
        QrCode = 1,
        UrlPost = 2,
        UrlGet = 3,
        UrlScheme = 4,
        Token =5,
    }
    
    /// <summary>
    /// 中国银行
    /// </summary>
    public enum EChinaBank
    {
        /// <summary>
        /// 工商银行
        /// </summary>
        [Description("工商银行")] ICBC,

        /// <summary>
        /// 农业银行
        /// </summary>
        [Description("农业银行")] ABC,

        /// <summary>
        /// 建设银行
        /// </summary>
        [Description("建设银行")] CCB,

        /// <summary>
        /// 交通银行
        /// </summary>
        [Description("交通银行")] BCOM,

        /// <summary>
        /// 中国银行
        /// </summary>
        [Description("中国银行")] BOC,

        /// <summary>
        /// 邮政储蓄银行
        /// </summary>
        [Description("邮政储蓄银行")] PSBC,


        /// <summary>
        /// 招商银行
        /// </summary>
        [Description("招商银行")] CMB,

        /// <summary>
        /// 民生银行
        /// </summary>
        [Description("民生银行")] CMBC,

        /// <summary>
        /// 中信银行
        /// </summary>
        [Description("中信银行")] CITIC,

        /// <summary>
        /// 浦发银行
        /// </summary>
        [Description("浦发银行")] SPDB,

        /// <summary>
        /// 兴业银行
        /// </summary>
        [Description("兴业银行")] CIB,

        /// <summary>
        /// 平安银行
        /// </summary>
        [Description("平安银行")] SZPAB,

        /// <summary>
        /// 广发银行
        /// </summary>
        [Description("广发银行")] GDB,

        /// <summary>
        /// 华夏银行
        /// </summary>
        [Description("华夏银行")] HXB,

        /// <summary>
        /// 浙商银行
        /// </summary>
        [Description("浙商银行")] CZB,

        /// <summary>
        /// 渤海银行
        /// </summary>
        [Description("渤海银行")] CBHB,

        /// <summary>
        /// 光大银行
        /// </summary>
        [Description("光大银行")] CEB,


        /// <summary>
        /// 北京银行
        /// </summary>
        [Description("北京银行")] BOBJ,

        /// <summary>
        /// 上海银行
        /// </summary>
        [Description("上海银行")] BOSH,

        /// <summary>
        /// 东亚银行
        /// </summary>
        [Description("东亚银行")] BEA,

        /// <summary>
        /// 南京银行
        /// </summary>
        [Description("南京银行")] NJCB,

        /// <summary>
        /// 江苏银行
        /// </summary>
        [Description("江苏银行")] JSB,

        /// <summary>
        /// 广州银行
        /// </summary>
        [Description("广州银行")] GZCB,

        /// <summary>
        /// 温州银行
        /// </summary>
        [Description("温州银行")] WZCB,

        /// <summary>
        /// 宁波银行
        /// </summary>
        [Description("宁波银行")] NBCB,

        /// <summary>
        /// 杭州银行
        /// </summary>
        [Description("杭州银行")] HZCB,

        /// <summary>
        /// 徽商银行
        /// </summary>
        [Description("徽商银行")] HSB,

        /// <summary>
        /// 上海农商行
        /// </summary>
        [Description("上海农商行")] SRCB,

        /// <summary>
        /// 北京农商行
        /// </summary>
        [Description("北京农商行")] BRCB,

        /// <summary>
        /// 深圳发展银行
        /// </summary>
        [Description("深圳发展银行")] SDB,

        /// <summary>
        /// 天津银行
        /// </summary>
        [Description("天津银行")] TCCB,

        /// <summary>
        /// 成都银行
        /// </summary>
        [Description("成都银行")] BOCD,


        /// <summary>
        /// 微信支付
        /// </summary>
        [Description("微信支付")] WXPAY,

        /// <summary>
        /// 支付宝
        /// </summary>
        [Description("支付宝")] ALIPAY
    }
    
    /// <summary>
    /// http://www.xe.com/zh-CN/currency/
    /// </summary>
    public enum ECurrency
    {
        [Description("美元")] USD,
        [Description("欧元")] EUR,
        [Description("英镑")] GBP,
        [Description("印度卢比")] INR,
        [Description("澳大利亚元")] AUD,
        [Description("加拿大元")] CAD,
        [Description("新加坡元")] SGD,
        [Description("瑞士法郎")] CHF,
        [Description("马来西亚林吉特")] MYR,
        [Description("日元")] JPY,
        [Description("中国人民币")] CNY,
        [Description("新西兰元")] NZD,
        [Description("泰铢")] THB,
        [Description("匈牙利福林")] HUF,
        [Description("阿联酋迪拉姆")] AED,
        [Description("港币")] HKD,
        [Description("墨西哥比索")] MXN,
        [Description("南非兰特")] ZAR,
        [Description("菲律宾比索")] PHP,
        [Description("瑞典克朗")] SEK,
        [Description("印度尼西亚卢比")] IDR,
        [Description("沙特里亚尔")] SAR,
        [Description("巴西雷亚尔")] BRL,
        [Description("土耳其里拉")] TRY,
        [Description("肯尼亚先令")] KES,
        [Description("韩元")] KRW,
        [Description("埃及镑")] EGP,
        [Description("伊拉克第纳尔")] IQD,
        [Description("挪威克朗")] NOK,
        [Description("科威特第纳尔")] KWD,
        [Description("俄罗斯卢布")] RUB,
        [Description("丹麦克朗")] DKK,
        [Description("巴基斯坦卢比")] PKR,
        [Description("以色列谢克尔")] ILS,
        [Description("波兰兹罗提")] PLN,
        [Description("卡塔尔里亚尔")] QAR,
        [Description("阿曼里亚尔")] OMR,
        [Description("哥伦比亚比索")] COP,
        [Description("智利比索")] CLP,
        [Description("新台币")] TWD,
        [Description("阿根廷比索")] ARS,
        [Description("捷克克朗")] CZK,
        [Description("越南盾")] VND,
        [Description("摩洛哥迪拉姆")] MAD,
        [Description("约旦第纳尔")] JOD,
        [Description("巴林第纳尔")] BHD,
        [Description("CFA 法郎")] XOF,
        [Description("斯里兰卡卢比")] LKR,
        [Description("乌克兰格里夫纳")] UAH,
        [Description("尼日利亚奈拉")] NGN,
        [Description("突尼斯第纳尔")] TND,
        [Description("乌干达先令")] UGX,
        [Description("罗马尼亚新列伊")] RON,
        [Description("孟加拉国塔卡")] BDT,
        [Description("秘鲁索尔")] PEN,
        [Description("格鲁吉亚拉里")] GEL,
        [Description("中非金融合作法郎")] XAF,
        [Description("斐济元")] FJD,
        [Description("委内瑞拉玻利瓦尔")] VEF,
        [Description("白俄罗斯卢布")] BYN,
        [Description("克罗地亚库纳")] HRK,
        [Description("乌兹别克斯坦索姆")] UZS,
        [Description("保加利亚列弗")] BGN,
        [Description("阿尔及利亚第纳尔")] DZD,
        [Description("伊朗里亚尔")] IRR,
        [Description("多米尼加比索")] DOP,
        [Description("冰岛克朗")] ISK,
        [Description("哥斯达黎加科朗")] CRC,
        [Description("叙利亚镑")] SYP,
        [Description("利比亚第纳尔")] LYD,
        [Description("牙买加元")] JMD,
        [Description("毛里求斯卢比")] MUR,
        [Description("加纳塞地")] GHS,
        [Description("安哥拉宽扎")] AOA,
        [Description("乌拉圭比索")] UYU,
        [Description("阿富汗尼")] AFN,
        [Description("黎巴嫩镑")] LBP,
        [Description("CFP 法郎")] XPF,
        [Description("特立尼达元")] TTD,
        [Description("坦桑尼亚先令")] TZS,
        [Description("阿尔巴尼列克")] ALL,
        [Description("东加勒比元")] XCD,
        [Description("危地马拉格查尔")] GTQ,
        [Description("尼泊尔卢比")] NPR,
        [Description("玻利维亚诺")] BOB,
        [Description("津巴布韦元")] ZWD,
        [Description("巴巴多斯元")] BBD,
        [Description("古巴可兑换比索")] CUC,
        [Description("老挝基普")] LAK,
        [Description("文莱元")] BND,
        [Description("博茨瓦纳普拉")] BWP,
        [Description("洪都拉斯伦皮拉")] HNL,
        [Description("巴拉圭瓜拉尼")] PYG,
        [Description("埃塞俄比亚比尔")] ETB,
        [Description("纳米比亚元")] NAD,
        [Description("巴布亚新几内亚基那")] PGK,
        [Description("苏丹镑")] SDG,
        [Description("澳门元")] MOP,
        [Description("尼加拉瓜科多巴")] NIO,
        [Description("百慕大元")] BMD,
        [Description("哈萨克斯坦坚戈")] KZT,
        [Description("巴拿马巴波亚")] PAB,
        [Description("波斯尼亚可兑换马尔卡")] BAM,
        [Description("圭亚那元")] GYD,
        [Description("也门里亚尔")] YER,
        [Description("马尔加什阿里亚")] MGA,
        [Description("开曼元")] KYD,
        [Description("莫桑比克梅蒂卡尔")] MZN,
        [Description("塞尔维亚第纳尔")] RSD,
        [Description("塞舌尔卢比")] SCR,
        [Description("亚美尼亚德拉姆")] AMD,
        [Description("所罗门群岛元")] SBD,
        [Description("阿塞拜疆马纳特")] AZN,
        [Description("塞拉利昂利昂")] SLL,
        [Description("汤加潘加")] TOP,
        [Description("伯利兹元")] BZD,
        [Description("马拉维克瓦查")] MWK,
        [Description("冈比亚达拉西")] GMD,
        [Description("布隆迪法郎")] BIF,
        [Description("索马里先令")] SOS,
        [Description("海地古德")] HTG,
        [Description("几内亚法郎")] GNF,
        [Description("马尔代夫拉菲亚")] MVR,
        [Description("蒙古图格里克")] MNT,
        [Description("刚果法郎")] CDF,
        [Description("圣多美多布拉")] STD,
        [Description("塔吉克斯坦索莫尼")] TJS,
        [Description("朝鲜元")] KPW,
        [Description("缅元")] MMK,
        [Description("巴索托洛蒂")] LSL,
        [Description("利比里亚元")] LRD,
        [Description("吉尔吉斯斯坦索姆")] KGS,
        [Description("直布罗陀镑")] GIP,
        [Description("摩尔多瓦列伊")] MDL,
        [Description("古巴比索")] CUP,
        [Description("柬埔寨瑞尔")] KHR,
        [Description("马其顿第纳尔")] MKD,
        [Description("瓦努阿图瓦图")] VUV,
        [Description("毛里塔尼亚乌吉亚")] MRO,
        [Description("荷兰盾")] ANG,
        [Description("斯威士兰里兰吉尼")] SZL,
        [Description("佛得角埃斯库多")] CVE,
        [Description("苏里南元")] SRD,
        [Description("萨尔瓦多科朗")] SVC,
        [Description("巴哈马元")] BSD,
        [Description("卢旺达法郎")] RWF,
        [Description("阿鲁巴或荷兰盾")] AWG,
        [Description("吉布提法郎")] DJF,
        [Description("不丹努尔特鲁姆")] BTN,
        [Description("科摩罗法郎")] KMF,
        [Description("萨摩亚塔拉")] WST,
        [Description("塞波加大公国 Luigino")] SPL,
        [Description("厄立特里亚纳克法")] ERN,
        [Description("福克兰群岛镑")] FKP,
        [Description("圣赫勒拿镑")] SHP,
        [Description("泽西岛镑")] JEP,
        [Description("土库曼斯坦马纳特")] TMT,
        [Description("图瓦卢元")] TVD,
        [Description("曼岛镑")] IMP,
        [Description("根西岛镑")] GGP,
        [Description("赞比亚克瓦查")] ZMW,

        [Description("白俄罗斯卢布")] BYR, //new
        [Description("比特币")] BTC, //new

        //[Description("国际货币基金组织特别提款权")] XDR,
        //[Description("金（盎司）")] XAU,
        //[Description("银（盎司）")] XAG,
        //[Description("铂（盎司）")] XPT,
        //[Description("钯（盎司）")] XPD,

        /// <summary>
        /// //////////////////////////////////////////////
        /// </summary>

        //[Description("阿联酋迪拉姆")] AED,
        //[Description("阿富汗尼")] AFN,
        //[Description("亚美尼亚德拉姆")] AMD,
        //[Description("荷属安的列斯盾")] ANG,
        //[Description("安哥拉宽扎")] AOA,
        //[Description("阿根廷披索")] ARS,
        //[Description("澳元")] AUD,
        //[Description("阿鲁巴盾")] AWG,
        //[Description("阿塞拜疆马纳特")] AZN,
        //[Description("波黑可兑换马克")] BAM,
        //[Description("巴巴多斯元")] BBD,
        //[Description("孟加拉塔卡")] BDT,
        //[Description("保加利亚列弗")] BGN,
        //[Description("巴林第纳尔")] BHD,
        //[Description("布隆迪法郎")] BIF,
        //[Description("百慕大元")] BMD,
        //[Description("文莱元")] BND,
        //[Description("玻利维亚 诺")] BOB,
        //[Description("巴西雷亚尔")] BRL,
        //[Description("巴哈马元")] BSD,
        //[Description("不丹努尔特鲁姆")] BTN,
        //[Description("博茨瓦纳普拉")] BWP,
        //[Description("白俄罗斯卢布")] BYR,
        //[Description("伯利兹元")] BZD,
        //[Description("加拿大元")] CAD,
        //[Description("刚果法郎")] CDF,
        //[Description("瑞士法郎")] CHF,
        //[Description("智利比索")] CLP,
        //[Description("人民币元")] CNY,
        //[Description("哥伦比亚比索")] COP,
        //[Description("哥斯达黎加科朗")] CRC,
        //[Description("捷克克朗")] CSK,
        //[Description("佛得角埃斯库多")] CVE,
        //[Description("捷克克鲁")] CZK,
        //[Description("吉布提法郎")] DJF,
        //[Description("丹麦克朗")] DKK,
        //[Description("多米尼加比索")] DOP,
        //[Description("阿尔及利亚第纳尔")] DZD,
        //[Description("爱沙尼亚克朗")] EEK,
        //[Description("埃及镑")] EGP,
        //[Description("厄立特里亚纳克法")] ERN,
        //[Description("埃塞俄比亚比尔")] ETB,
        //[Description("欧元")] EUR,
        //[Description("斐济元")] FJD,
        //[Description("福克兰群岛镑")] FKP,
        //[Description("英镑")] GBP,
        //[Description("格鲁吉亚拉里")] GEL,
        //[Description("加纳塞地")] GHS,
        //[Description("直布罗陀镑")] GIP,
        //[Description("冈比亚达拉西")] GMD,
        //[Description("几内亚法郎")] GNF,
        //[Description("危地马拉格查尔")] GTQ,
        //[Description("圭亚那元")] GYD,
        //[Description("香港港元")] HKD,
        //[Description("洪都拉斯伦皮拉")] HNL,
        //[Description("克罗地亚库纳")] HRK,
        //[Description("海地古德")] HTG,
        //[Description("匈牙利货币福林")] HUF,
        //[Description("印度尼西亚卢比")] IDR,
        //[Description("以色列新谢克尔")] ILS,
        //[Description("印度卢比")] INR,
        //[Description("冰岛克郎")] ISK,
        //[Description("牙买加元")] JMD,
        //[Description("约旦第纳尔")] JOD,
        //[Description("日元")] JPY,
        //[Description("肯尼亚先令")] KES,
        //[Description("吉尔吉斯斯坦索姆")] KGS,
        //[Description("柬埔寨瑞尔")] KHR,
        //[Description("刚果法郎")] KMF,
        //[Description("韩元")] KRW,
        //[Description("科威特第纳尔")] KWD,
        //[Description("开曼群岛元")] KYD,
        //[Description("哈萨克斯坦腾格")] KZT,
        //[Description("老挝基普")] LAK,
        //[Description("黎巴嫩镑")] LBP,
        //[Description("斯里兰卡鲁普")] LKR,
        //[Description("利比里亚元")] LRD,
        //[Description("立陶宛立特")] LTL,
        //[Description("拉脱维亚拉特")] LVL,
        //[Description("摩洛哥迪拉姆")] MAD,
        //[Description("摩尔多瓦列伊")] MDL,
        //[Description("马达加斯加阿里亚里")] MGA,
        //[Description("马其顿第纳尔")] MKD,
        //[Description("缅甸元")] MMK,
        //[Description("蒙古图格里克")] MNT,
        //[Description("澳门元")] MOP,
        //[Description("毛里塔尼亚乌吉亚")] MRO,
        //[Description("毛里求斯卢比")] MUR,
        //[Description("马尔代夫拉菲亚")] MVR,
        //[Description("马拉维克瓦查")] MWK,
        //[Description("墨西哥比索")] MXN,
        //[Description("马来西亚林吉特")] MYR,
        //[Description("莫桑比克梅蒂卡尔")] MZN,
        //[Description("纳米比亚元")] NAD,
        //[Description("尼日利亚奈拉")] NGN,
        //[Description("尼加拉瓜金科多巴")] NIO,
        //[Description("挪威克朗")] NOK,
        //[Description("尼泊尔卢比")] NPR,
        //[Description("新西兰元")] NZD,
        //[Description("阿曼里亚尔")] OMR,
        //[Description("巴拿马巴波亚")] PAB,
        //[Description("秘鲁索尔")] PEN,
        //[Description("巴布亚新几内亚基纳")] PGK,
        //[Description("菲律宾披索")] PHP,
        //[Description("巴基斯坦卢比")] PKR,
        //[Description("波兰兹罗提")] PLN,
        //[Description("巴拉圭瓜拉尼")] PYG,
        //[Description("卡塔尔里亚尔")] QAR,
        //[Description("罗马尼亚列伊")] RON,
        //[Description("塞尔维亚第纳尔")] RSD,
        //[Description("俄罗斯卢布")] RUB,
        //[Description("卢旺达法郎")] RWF,
        //[Description("沙特里亚尔")] SAR,
        //[Description("所罗门岛元")] SBD,
        //[Description("塞舌尔卢比")] SCR,
        //[Description("瑞典克郎")] SEK,
        //[Description("新加坡元")] SGD,
        //[Description("圣赫勒拿镑")] SHP,
        //[Description("斯洛伐克克朗")] SKK,
        //[Description("塞拉利昂利昂")] SLL,
        //[Description("索马里先令")] SOS,
        //[Description("苏里南元")] SRD,
        //[Description("圣多美和普林西比多布拉")] STD,
        //[Description("叙利亚镑")] SYP,
        //[Description("斯威士兰里兰吉尼")] SZL,
        //[Description("泰铢")] THB,
        //[Description("塔吉克斯坦索莫尼")] TJS,
        //[Description("突尼斯第纳尔")] TND,
        //[Description("汤加潘加")] TOP,
        //[Description("土耳其里拉")] TRY,
        //[Description("特立尼达多巴哥元")] TTD,
        //[Description("台币")] TWD,
        //[Description("坦桑尼亚先令")] TZS,
        //[Description("乌克兰格里夫尼亚")] UAH,
        //[Description("乌干达先令")] UGX,
        //[Description("美元")] USD,
        //[Description("乌拉圭新比索")] UYU,
        //[Description("乌兹别克斯坦苏姆")] UZS,
        //[Description("委内瑞拉玻利瓦尔")] VEF,
        //[Description("越南盾")] VND,
        //[Description("瓦努阿图瓦")] VUV,
        //[Description("萨摩亚塔拉")] WST,
        //[Description("中非金融合作法郎")] XAF,
        //[Description("东加勒比元")] XCD,
        //[Description("西非法郎")] XOF,
        //[Description("太平洋结算法郎")] XPF,
        //[Description("也门里亚尔")] YER,
        //[Description("南非兰特")] ZAR,
        //[Description("赞比亚克瓦查")] ZMW,
        //[Description("津巴布韦元")] ZWD
    }
    
}
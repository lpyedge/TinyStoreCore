using System.ComponentModel;
using System.Runtime.Serialization;

namespace TinyStore
{
    public enum EUserLogType : int
    {
        注册 = 1,
        登录 = 2,
        修改商户信息 = 3,
        修改店铺信息 = 4,
        分类管理 = 5,
        产品管理 = 6,
        订单管理 = 7,
        优惠码管理 = 8,
        库存管理 = 9,
        提现 = 10,
        货源管理 = 11,
        结算记录 = 12
    }

    public enum EAdminLogType : int
    {
        登录 = 1,
        管理员管理 = 2,
        订单管理 = 3,
        提现管理 = 4,
        商户管理 = 5,
        店铺管理 = 6
    }

    public enum EBankType : int
    {
        支付宝 = 1,
        微信 = 2,
        银联 = 3,
        
        其它 = 9,

        [BankMark(Global.AlipaySchema.EBankMark.ICBC)]
        工商银行 = 11,
        [BankMark(Global.AlipaySchema.EBankMark.ABC)]
        农业银行 = 12,
        [BankMark(Global.AlipaySchema.EBankMark.CCB)]
        建设银行 = 13,
        [BankMark(Global.AlipaySchema.EBankMark.BOC)]
        中国银行 = 14,
        [BankMark(Global.AlipaySchema.EBankMark.BCM)]
        交通银行 = 15,
        [BankMark(Global.AlipaySchema.EBankMark.PSBC)]
        邮储银行 = 16,

        // 招商银行 = 51,
        // 民生银行 = 52,
        // 中信银行 = 53,
        // 浦发银行 = 54,
        // 兴业银行 = 55,
        // 平安银行 = 56,
        // 广发银行 = 57,
        // 华夏银行 = 58,
        // 光大银行 = 59,
        // 浙商银行 = 60,
        // 渤海银行 = 61,
    }

    public enum EUserLevel : int
    {
        无 = 0,
        一星 = 1,
        二星 = 2,
        三星 = 3,
        合作商 = 9
    }

    /// <summary>
    /// 发货方式
    /// </summary>
    public enum EDeliveryType : int
    {
        卡密 = 1,
        人工 = 2,
        接口 = 3,
    }

    public enum EStoreTemplate : int
    {
        模板一 = 1,
        模板二 = 2,
        模板三 = 3,
        模板四 = 4,
        模板五 = 5,
    }

    public enum EBillType : int
    {
        收款 = 1,
        退款 = 2,
        
        成本结算 = 10,
        
        充值 = 20,
        提现 = 21,
        
        交易手续费 = 22,
    }
    
    public enum EPaymentType : int
    {
        [PaymentAttribute(0.006, "支付宝H5","手机端调用")] 
        AliPayWap = 1,
        [PaymentAttribute(0.006, "微信H5","手机端调用")] 
        WeChatH5 = 2,
        [PaymentAttribute(0.006, "支付宝扫码","电脑端调用")] 
        AliPayQR = 3,
        [PaymentAttribute(0.006, "微信扫码","电脑端调用")] 
        WeChatQR = 4,
    }
    
    // 分割线
    
    
    public enum EState : int
    {
        客户下单 = 1,
        等待发货 = 3,
        完成订单 = 5,
    }

    public enum EOrderSearchKey : int
    {
        订单号 = 1,
        联系方式 = 2,
        商品名称 = 3,
        商品卡号 = 4,
        交易编号 = 5,
        备注 = 6
    }



    public enum EWithDrawState : int
    {
        未处理 = 1,
        提现成功 = 2,
        提现失败 = 3
    }

    public enum EOrderTimeType : int
    {
        最后变动日期 = 1,
        付款日期 = 2,
        到期日期 = 3
    }
}
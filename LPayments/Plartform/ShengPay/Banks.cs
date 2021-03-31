using System;
using System.Collections.Generic;
using System.Net;

namespace LPayments.Plartform.ShengPay
{
    [PayChannel(EChannel.ChinaBank)]
    public class Banks : ShengPay
    {
        public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BEA] = "BEA",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "COMM",
            [EChinaBank.BOSH] = "BOS",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.GZCB] = "GZCB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZCB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "BON",
            [EChinaBank.PSBC] = "POSTGC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SHRCB",
            [EChinaBank.SZPAB] = "SZPAB",
            [EChinaBank.WZCB] = "WZCB",
            [EChinaBank.BRCB] = "BJRCB",
        };

        public Banks() : base()
        {
            m_PayChannel = "";
        }

        public Banks(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_PayChannel = "";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }

        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
        }
    }

    [PayChannel(EChannel.ChinaBankCredit)]
    public class BanksCredit : ShengPay
    {
        public Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BEA] = "BEA",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "COMM",
            [EChinaBank.BOSH] = "BOS",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.GZCB] = "GZCB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZCB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "BON",
            [EChinaBank.PSBC] = "POSTGC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SHRCB",
            [EChinaBank.SZPAB] = "SZPAB",
            [EChinaBank.WZCB] = "WZCB",
            [EChinaBank.BRCB] = "BJRCB",
        };

        public BanksCredit() : base()
        {
            m_PayChannel = "20";
        }

        public BanksCredit(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_PayChannel = "20";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }

        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (extend_params == null) throw new ArgumentNullException("extend_params");
            try
            {
                var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
                m_InstCode = BankDic[extend.Bank];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
            }
            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
        }
    }

    [PayChannel(EChannel.ChinaBankDebit)]
    public class BanksDebit : ShengPay
    {
        public static Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BEA] = "BEA",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "COMM",
            [EChinaBank.BOSH] = "BOS",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.GZCB] = "GZCB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZCB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "BON",
            [EChinaBank.PSBC] = "POSTGC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SHRCB",
            [EChinaBank.SZPAB] = "SZPAB",
            [EChinaBank.WZCB] = "WZCB",
            [EChinaBank.BRCB] = "BJRCB",
        };

        public BanksDebit() : base()
        {
            m_PayChannel = "19";
        }

        public BanksDebit(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_PayChannel = "19";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }


        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (extend_params == null) throw new ArgumentNullException("extend_params");
            try
            {
                var extend = Utils.Json.Deserialize<PayExtend>(Utils.Json.Serialize(extend_params));
                m_InstCode = BankDic[extend.Bank];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend", "extend_params");
            }
            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl, p_CancelUrl);
        }
    }
}
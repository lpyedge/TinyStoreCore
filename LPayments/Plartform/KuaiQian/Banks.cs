using System;
using System.Collections.Generic;
using System.Net;

namespace LPayments.Plartform.KuaiQian
{
    [PayChannel(EChannel.ChinaBank)]
    public class Banks : KuaiQian
    {
        internal Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "BCOM",
            [EChinaBank.BOSH] = "SHB",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "NJCB",
            [EChinaBank.PSBC] = "PSBC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SRCB",
            [EChinaBank.BRCB] = "BJRCB",
            [EChinaBank.SZPAB] = "PAB",
        };

        public Banks() : base()
        {
            m_payType = "10";
        }

        public Banks(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_payType = "10";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }

        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName,
            IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "",
            dynamic extend_params = null)
        {
            if (extend_params == null) throw new ArgumentNullException("extend_params");
            try
            {
                var extend =
                    Utils.JsonUtility.Deserialize<PayExtend>(
                        Utils.JsonUtility.Serialize(extend_params));
                if (!BankDic.ContainsKey(extend.Bank))
                    throw new ArgumentException("Bank : " + extend.Bank.ToString() + " dose not support");
                m_bankId = BankDic[extend.Bank];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend",
                    "extend_params");
            }

            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl,
                p_CancelUrl);
        }
    }

    [PayChannel(EChannel.ChinaBankCredit)]
    public class BanksCredit : KuaiQian
    {
        internal Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "BCOM",
            [EChinaBank.BOSH] = "SHB",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "NJCB",
            [EChinaBank.PSBC] = "PSBC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SRCB",
            [EChinaBank.BRCB] = "BJRCB",
            [EChinaBank.SZPAB] = "PAB",
        };

        public BanksCredit() : base()
        {
            m_payType = "10-1";
        }

        public BanksCredit(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_payType = "10-1";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }

        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName,
            IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "",
            dynamic extend_params = null)
        {
            if (extend_params == null) throw new ArgumentNullException("extend_params");
            try
            {
                var extend =
                    Utils.JsonUtility.Deserialize<PayExtend>(
                        Utils.JsonUtility.Serialize(extend_params));
                if (!BankDic.ContainsKey(extend.Bank))
                    throw new ArgumentException("Bank : " + extend.Bank.ToString() + " dose not support");
                m_bankId = BankDic[extend.Bank];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend",
                    "extend_params");
            }

            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl,
                p_CancelUrl);
        }
    }

    [PayChannel(EChannel.ChinaBankDebit)]
    public class BanksDebit : KuaiQian
    {
        internal Dictionary<EChinaBank, string> BankDic = new Dictionary<EChinaBank, string>()
        {
            [EChinaBank.ABC] = "ABC",
            [EChinaBank.BOBJ] = "BOB",
            [EChinaBank.BOC] = "BOC",
            [EChinaBank.BCOM] = "BCOM",
            [EChinaBank.BOSH] = "SHB",
            [EChinaBank.CBHB] = "CBHB",
            [EChinaBank.CCB] = "CCB",
            [EChinaBank.CEB] = "CEB",
            [EChinaBank.CIB] = "CIB",
            [EChinaBank.CITIC] = "CITIC",
            [EChinaBank.CMB] = "CMB",
            [EChinaBank.CMBC] = "CMBC",
            [EChinaBank.CZB] = "CZB",
            [EChinaBank.GDB] = "GDB",
            [EChinaBank.HSB] = "HSB",
            [EChinaBank.HXB] = "HXB",
            [EChinaBank.HZCB] = "HZB",
            [EChinaBank.ICBC] = "ICBC",
            [EChinaBank.NBCB] = "NBCB",
            [EChinaBank.NJCB] = "NJCB",
            [EChinaBank.PSBC] = "PSBC",
            [EChinaBank.SPDB] = "SPDB",
            [EChinaBank.SRCB] = "SRCB",
            [EChinaBank.BRCB] = "BJRCB",
            [EChinaBank.SZPAB] = "PAB",
        };

        public BanksDebit() : base()
        {
            m_payType = "10-2";
        }

        public BanksDebit(string p_SettingsJson) : base(p_SettingsJson)
        {
            m_payType = "10-2";
        }

        public class PayExtend
        {
            public EChinaBank Bank { get; set; }
        }

        public override PayTicket Pay(string p_OrderId, double p_Amount, ECurrency p_Currency, string p_OrderName,
            IPAddress p_ClientIP = null, string p_ReturnUrl = "", string p_NotifyUrl = "", string p_CancelUrl = "",
            dynamic extend_params = null)
        {
            if (extend_params != null) throw new ArgumentNullException("extend_params");
            try
            {
                var extend =
                    Utils.JsonUtility.Deserialize<PayExtend>(
                        Utils.JsonUtility.Serialize(extend_params));
                if (!BankDic.ContainsKey(extend.Bank))
                    throw new ArgumentException("Bank : " + extend.Bank.ToString() + " dose not support");
                m_bankId = BankDic[extend.Bank];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("extend_params must be " + this.GetType().FullName + ".PayExtend",
                    "extend_params");
            }

            return base.Pay(p_OrderId, p_Amount, p_Currency, p_OrderName, p_ClientIP, p_ReturnUrl, p_NotifyUrl,
                p_CancelUrl);
        }
    }
}
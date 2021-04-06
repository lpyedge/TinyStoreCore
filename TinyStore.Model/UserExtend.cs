using System;

namespace TinyStore.Model
{
    [Serializable]
    public class UserExtendModel
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public int UserId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string TelPhone { get; set; } = "";

        /// <summary>
        /// 邮箱
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string Email { get; set; } = "";

        /// <summary>
        /// 联系QQ
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string QQ { get; set; } = "";

        /// <summary>
        /// 姓名
        /// </summary>
        [SqlSugar.SugarColumn(Length = 50)]
        public string Name { get; set; } = "";

        /// <summary>
        /// 身份证
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25)]
        public string IdCard { get; set; } = "";

        /// <summary>
        /// 收款账户类型
        /// </summary>
        public EBankType BankType { get; set; } = EBankType.支付宝;

        /// <summary>
        /// 收款账户名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25)]
        public string BankAccount { get; set; } = "";

        /// <summary>
        /// 收款人名称
        /// </summary>
        [SqlSugar.SugarColumn(Length = 25)]
        public string BankPersonName { get; set; } = "";

        /// <summary>
        /// 注册IP
        /// </summary>
        [SqlSugar.SugarColumn(Length = 45)]
        public string RegisterIP { get; set; } = "";

        /// <summary>
        /// 注册日期
        /// </summary>
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 客户端信息
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string UserAgent { get; set; } = "";

        /// <summary>
        /// 客户端语言
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string AcceptLanguage { get; set; } = "";
    }
}
using System;
using SqlSugar;

namespace TinyStore.Model
{
    [Serializable]
    public class UserLogModel
    {
        /// <summary>
        ///编号
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true,Length = 28)]
        public string UserLogId { get; set; }

        /// <summary>
        ///用户编号
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///注释
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string Memo { get; set; }

        /// <summary>
        /// 店铺编号
        /// </summary>
        [SqlSugar.SugarColumn(Length = 28)]
        public string StoreId { get; set; }

        /// <summary>
        ///客户端IP
        /// </summary>
        [SqlSugar.SugarColumn(Length = 45)]
        public string ClientIP { get; set; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        [SqlSugar.SugarColumn(Length = 500)]
        public string UserAgent { get; set; }

        /// <summary>
        /// 客户端语言
        /// </summary>
        [SqlSugar.SugarColumn(Length = 100)]
        public string AcceptLanguage { get; set; }

        /// <summary>
        ///用户日志类型
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"UserLogType"})]
        public EUserLogType UserLogType { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        [SqlSugar.SugarColumn(IndexGroupNameList = new []{"CreateDate"})]
        public DateTime CreateDate { get; set; }
        
        
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string Account { get; set; }
    }
}
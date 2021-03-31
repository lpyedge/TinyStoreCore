namespace LPayments
{
    public class Setting
    {
        public Setting()
        {
            Description = "";
            Value = "";
        }
        public Setting(string p_Name)
        {
            Name = p_Name;
            Description = "";
            Value = "";
        }

        /// <summary>
        /// 设置名称
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 设置正则匹配
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore()]
        public string Regex { get; internal set; }

        /// <summary>
        /// 设置描述
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore()]
        public string Description { get; internal set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore()]
        public bool Requied { get; internal set; }

        /// <summary>
        /// 设置值
        /// </summary>
        public string Value { get; set; }
    }
}
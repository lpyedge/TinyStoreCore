using System.Collections.Generic;
using System.Linq;

namespace Payments
{
    public class SettingBase
    {
        public SettingBase()
        {
        }
        public SettingBase(string p_Name)
        {
            Name = p_Name;
            Value = "";
        }

        /// <summary>
        /// 设置名称
        /// </summary>
        public string Name { get; set; }

      
        /// <summary>
        /// 设置值
        /// </summary>
        public string Value { get; set; }
       
    }

    public class Setting: SettingBase
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
        /// 设置正则匹配
        /// </summary>
        public string Regex { get; internal set; }

        /// <summary>
        /// 设置描述
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Requied { get; internal set; }


        public static List<SettingBase> ToSettingBase(List<Setting> settingList)
        {
            return settingList.Select(p=> (SettingBase)p).ToList();
        }
    }
}
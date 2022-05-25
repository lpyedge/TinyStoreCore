using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Payments
{
    public static class Context
    {
        public static readonly ConcurrentDictionary<string,Type>
            PayChannelDictionary = new ();

        static Context()
        {
            var extAssembly = Assembly.GetExecutingAssembly();
            PayChannelDictionary = new ();;
            foreach (var item in extAssembly.GetTypes().Where(p=>p.IsClass && p.IsPublic ))
            {
                if (item.TypeChannelAttribute() != null)
                {
                    var payChannel = (IPayChannel)Utils.Core.CreateInstance(item);
                    PayChannelDictionary[payChannel.Name] = item;
                }
            }
        }

        public static PayChannelAttribute TypeChannelAttribute(this Type p_Type)
        {
            var cas = Utils.Core.TypeAttribute<PayChannelAttribute>(p_Type);
            return cas?.Count == 1 ? cas[0] : null;
        }


        public static IPayChannel Get(string name, string settingJson = "")
        {
            if (PayChannelDictionary.ContainsKey(name) )
            {
                var data = (IPayChannel)Utils.Core.CreateInstance(PayChannelDictionary[name]);
                data.SettingsJson = settingJson;
                return data;
            }
            
            return null;
        } 
        
        public static IPayChannel Get(EPlatform ePlatform, EChannel eChannel, EPayType ePayType = EPayType.PC,
            string settingJson = "")
        {
            
            var name = ePlatform.ToString()
                       + "_" + eChannel.ToString()
                       + "_" + ePayType.ToString();
            return Get(name,settingJson);
        }
        
        public static IPayChannel Get(string name, List<SettingBase> settings)
        {
            if(settings!=null)
                return Get(name,Utils.JsonUtility.Serialize(settings));
            return null;
        }
        
        public static IPayChannel Get(EPlatform ePlatform, EChannel eChannel, EPayType ePayType, List<SettingBase> settings)
        {
            
            var name = ePlatform.ToString()
                       + "_" + eChannel.ToString()
                       + "_" + ePayType.ToString();
            return Get(name,settings);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyStore
{
    /// <summary>
    /// 插件基类
    /// </summary>
    /// <typeparam name="T">插件主父类(抽象类/abstract)</typeparam>
    /// <typeparam name="K">插件子对象标识类型</typeparam>
    /// <typeparam name="C">插件子对象配置类</typeparam>
    public abstract class IPlugin<T, K, C> where T : IPlugin<T, K, C> where C : class, new()
    {
        //AppDomain.CurrentDomain.BaseDirectory 当前程序所属基目录  x:\ProjectXXX\bin\Debug\
        //System.Environment.CurrentDirectory   启动环境目录(该进程从中启动的目录)的完全限定目录。x:\ProjectXXX
        //System.IO.Directory.GetCurrentDirectory() 获取应用程序的当前工作目录。x:\ProjectXXX


        protected static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory + @"Plugin\";

        /// <summary>
        /// 插件子类配置信息路径
        /// </summary>
        public static readonly string ConfigsPath = BasePath + @"{0}\Configs\";

        /// <summary>
        /// 插件日志信息路径
        /// 对应目录存在判断是否产生日志
        /// </summary>
        public static readonly string LogsPath = BasePath + @"{0}\Logs\";

        static IPlugin()
        {
            Instances = new Dictionary<K, T>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.ExportedTypes.Where(p => IsBaseOn<T>(p) && !p.IsAbstract))
                {
                    var instance = (Activator.CreateInstance(type) as T);
                    Instances.Add(instance.Key, instance);
                }
            }

            //同步更新实体对象的配置信息
            var configs = LoadConfigs();
            foreach (var item in Instances)
            {
                if (configs.ContainsKey(item.Key))
                {
                    SetConfig(item.Key, configs[item.Key]);
                }
                else
                {
                    if (item.Value.Config != default(C))
                    {
                        SetConfig(item.Key, item.Value.Config);
                    }
                    else
                    {
                        SetConfig(item.Key, new C());
                    }
                }
            }

            //插件主父类的静态初始化方法补充执行
            //因为C#调用原理会由子级向上逐级调用
            //而初次调用主父类静态初始化方法时所有属性和程序集都未加载(因为只有执行到插件基类时才加载)
            //所以如果需要插件主父类正常静态初始化需要再原始静态初始化时实现StaticInit方法
            //会在插件基类初始化某个插件主类的所有对象和配置信息之后执行
            StaticInit?.Invoke();
        }

        protected static bool IsBaseOn<BT>(Type type)
        {
            bool res = false;
            while (type.BaseType != null && !res)
            {
                if (type.BaseType == typeof(BT))
                {
                    res = true;
                }
                else
                {
                    type = type.BaseType;
                }
            }
            return res;
        }

        protected static Action StaticInit { get; set; }

        public abstract K Key { get; }

        public static Dictionary<K, T> Instances { get; protected set; }
        public virtual C Config { get; protected set; }


        #region Config

        protected static readonly string ConfigPath = string.Format(ConfigsPath, typeof(T).Name);

        private static Dictionary<K, C> LoadConfigs()
        {
            var configDic = new Dictionary<K, C>();
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            else
            {
                foreach (var filepath in Directory.GetFiles(ConfigPath))
                {
                    var fileinfo = new FileInfo(filepath);
                    var name = fileinfo.Name.Replace(fileinfo.Extension, string.Empty);
                    var instance = Instances.FirstOrDefault(p => p.Key.ToString() == name);
                    if (instance.Value != null)
                    {
                        var josnconfig = File.ReadAllText(fileinfo.FullName);
                        configDic.Add(instance.Key,
                            System.Text.Json.JsonSerializer.Deserialize<C>(josnconfig));
                    }
                }
            }

            return configDic;
        }

        public static void SetConfig(K key, C config)
        {
            if (Instances.ContainsKey(key))
            {
                Instances[key].Config = config;


                //配置信息变化重新执行子方法的静态初始化方法
                if (StaticInit != null)
                    StaticInit.Invoke();

                File.WriteAllText(ConfigPath + key.ToString() + ".json",
                    System.Text.Json.JsonSerializer.Serialize(config));
            }
        }

        #endregion

        #region Log

        protected static readonly string LogPath = string.Format(LogsPath, typeof(T).Name);

        static readonly bool _EnableLog = Directory.Exists(LogPath);

        public static void Log(string content, string name)
        {
            if (_EnableLog)
            {
                FileSave(LogPath + name + DateTime.Now.ToString("-yyyyMMdd") + ".log", content);
            }
        }


        protected static void FileSave(string p_Path, string p_Content, bool p_IsAppend = true)
        {
            FileSave(p_Path, Encoding.UTF8.GetBytes(p_Content), p_IsAppend);
        }

        protected static void FileSave(string p_Path, byte[] p_Bytes, bool p_IsAppend = true)
        {
            Task.Factory.StartNew((state) =>
            {
                byte[] buff = ((dynamic)state).Buff;
                bool isappend = ((dynamic)state).IsAppend;
                FileInfo fileinfo = ((dynamic)state).FileInfo;
                try
                {
                    if (!fileinfo.Directory.Exists)
                    {
                        fileinfo.Directory.Create();
                    }

                    var filemode = isappend ? FileMode.Append : FileMode.Create;

                    using (var fs = new FileStream(fileinfo.FullName, filemode))
                    {
                        fs.Seek(0, SeekOrigin.End);
                        fs.Write(buff, 0, buff.Length);
                        fs.Flush();
                    }
                }
                catch
                { }
            }, new { FileInfo = new FileInfo(p_Path), Buff = p_Bytes, IsAppend = p_IsAppend });
        }

        #endregion
    }
  
    /// <summary>
    /// 插件基类
    /// </summary>
    /// <typeparam name="T">插件主父类(抽象类/abstract)</typeparam>
    /// <typeparam name="K">插件子对象标识类型</typeparam>
    /// <typeparam name="C">插件子对象配置类</typeparam>
    /// <typeparam name="S">插件主父类全局配置类</typeparam>
    public abstract class IPlugin<T, K, C, S> : IPlugin<T, K, C> where T : IPlugin<T, K, C, S> where C : class, new() where S : class, new()
    {
        static IPlugin()
        {
            //同步更新实全局配置信息
            var setting = LoadSetting();
            if (setting != default(S))
            {
                SetSetting(setting);
            }
            else
            {
                SetSetting(new S());
            }
        }

        public static S Setting { get; protected set; }

        #region Setting

        /// <summary>
        /// 插件类全局配置信息路径
        /// </summary>
        public static readonly string SettingsPath = BasePath + @"{0}\";

        const string SettingFileName = "Setting.json";

        protected static readonly string SettingPath = string.Format(SettingsPath, typeof(T).Name);

        private static S LoadSetting()
        {
            var setting = default(S);
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            else
            {
                var fileinfo = new FileInfo(SettingPath + SettingFileName);
                if (fileinfo.Exists)
                {
                    var josnconfig = File.ReadAllText(fileinfo.FullName);
                    setting = System.Text.Json.JsonSerializer.Deserialize<S>(josnconfig);
                }
            }
            return setting;
        }

        public static void SetSetting(S setting)
        {
            Setting = setting;

            File.WriteAllText(SettingPath + SettingFileName, System.Text.Json.JsonSerializer.Serialize(setting));
        }

        #endregion
    }
}

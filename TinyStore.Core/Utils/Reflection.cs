using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CSharp;

namespace TinyStore.Utils
{
    /// <summary>
    /// Reflection 的摘要说明。
    /// </summary>
    public static class Reflection
    {
        #region Assembly程序集相关方法

        /// <summary>
        /// 程序集是否Debug版本
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool AssemblyDebug(Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>()
                .Any(debuggableAttribute => debuggableAttribute.IsJITTrackingEnabled);
        }

        /// <summary>
        /// 无占用方式通过路径调用程序集。不用加载当前程序集要调用的其他程序集！
        /// </summary>
        /// <param name="p_AssemblyFilePath"></param>
        /// <returns></returns>
        public static Assembly AssemblyLoadFile(string p_AssemblyFilePath)
        {
            Assembly resAssembly = null;
            if (File.Exists(p_AssemblyFilePath))
            {
                byte[] data = File.ReadAllBytes(p_AssemblyFilePath);
                resAssembly = Assembly.Load(data);
                //AppDomain.CurrentDomain.ExecuteAssembly("");
            }

            return resAssembly;
        }


        /// <summary>
        /// 无占用方式通过路径调用程序集。不用加载当前程序集要调用的其他程序集！
        /// </summary>
        /// <param name="p_AssemblyFilePath"></param>
        /// <returns></returns>
        public static Assembly AssemblyLoad(string p_AssemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p =>
                p.FullName.IndexOf(p_AssemblyName + ",", StringComparison.Ordinal) == 0);
        }

        #endregion

        #region Type相关方法

        /// <summary>
        /// 不同对象之间的深拷贝，最好属性名一样
        /// </summary>
        /// <typeparam name="T">源对象类型</typeparam>
        /// <typeparam name="F">目的对象类型</typeparam>
        /// <param name="original">源对象</param>
        /// <returns>目的对象</returns>
        public static F DeepCopy<T, F>(T original)
        {
            var serializerT = new DataContractJsonSerializer(typeof(T));
            var serializerF = new DataContractJsonSerializer(typeof(F));
            string json;
            var stream = new MemoryStream();
            serializerT.WriteObject(stream, original);
            stream.Position = 0;
            using (var sr = new StreamReader(stream))
            {
                json = sr.ReadToEnd();
            }

            using (var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(json.ToCharArray())))
            {
                return (F) serializerF.ReadObject(stream1);
            }
        }


        #region IsSimpleType

        /// <summary>
        /// IsSimpleType 是否为简单类型：数值、字符、字符串、日期、布尔、枚举、Type
        /// </summary>      
        public static bool IsSimpleType(this Type p_Type)
        {
            if (IsNumbericType(p_Type))
            {
                return true;
            }

            if (p_Type == typeof(char))
            {
                return true;
            }

            if (p_Type == typeof(string))
            {
                return true;
            }


            if (p_Type == typeof(bool))
            {
                return true;
            }


            if (p_Type == typeof(DateTime))
            {
                return true;
            }

            if (p_Type == typeof(Type))
            {
                return true;
            }

            if (p_Type.IsEnum)
            {
                return true;
            }

            if (IsNullableType(p_Type))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IsNumbericType 是否为数值类型

        /// <summary>
        /// 是否为数值类型
        /// </summary>
        /// <param name="p_DestDataType"></param>
        /// <returns></returns>
        public static bool IsNumbericType(this Type p_DestDataType)
        {
            return (p_DestDataType == typeof(int)) || (p_DestDataType == typeof(uint)) ||
                   (p_DestDataType == typeof(double))
                   || (p_DestDataType == typeof(short)) || (p_DestDataType == typeof(ushort)) ||
                   (p_DestDataType == typeof(decimal))
                   || (p_DestDataType == typeof(long)) || (p_DestDataType == typeof(ulong)) ||
                   (p_DestDataType == typeof(float))
                   || (p_DestDataType == typeof(byte)) || (p_DestDataType == typeof(sbyte));
        }

        #endregion

        #region IsIntegerCompatibleType 是否为整数兼容类型

        /// <summary>
        /// 是否为整数兼容类型
        /// </summary>
        /// <param name="p_DestDataType"></param>
        /// <returns></returns>
        public static bool IsIntegerCompatibleType(this Type p_DestDataType)
        {
            return (p_DestDataType == typeof(int)) || (p_DestDataType == typeof(uint)) ||
                   (p_DestDataType == typeof(short)) || (p_DestDataType == typeof(ushort))
                   || (p_DestDataType == typeof(long)) || (p_DestDataType == typeof(ulong)) ||
                   (p_DestDataType == typeof(byte)) || (p_DestDataType == typeof(sbyte));
        }

        #endregion


        #region IsNullableType 是否为可以为null的值类型

        public static bool IsNullableType(Type p_DestDataType)
        {
            return p_DestDataType.IsGenericType && p_DestDataType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        #endregion

        #region ConvertType

        /// <summary>
        /// ChangeType 对System.Convert.ChangeType进行了增强，支持(0,1)到bool的转换，字符串->枚举、int->枚举、字符串->Type
        /// </summary>       
        public static object ConvertSimpleType<T>(this object p_Inputvalue)
        {
            return (T) ConvertSimpleType(p_Inputvalue, typeof(T));
        }

        /// <summary>
        /// ChangeType 对System.Convert.ChangeType进行了增强，支持(0,1)到bool的转换，字符串->枚举、int->枚举、字符串->Type
        /// </summary>       
        public static dynamic ConvertSimpleType(this object p_Inputvalue, Type p_DestinationType)
        {
            #region null

            if (p_Inputvalue == null)
            {
                return null;
            }

            #endregion

            #region Same Type

            if (p_DestinationType == p_Inputvalue.GetType())
            {
                return p_Inputvalue;
            }

            #endregion

            #region bool 1,0

            if (p_DestinationType == typeof(bool))
            {
                if (p_Inputvalue.ToString() == "0")
                {
                    return false;
                }

                if (p_Inputvalue.ToString() == "1")
                {
                    return true;
                }
            }

            #endregion

            #region Enum

            if (p_DestinationType.IsEnum)
            {
                int intVal;
                var suc = int.TryParse(p_Inputvalue.ToString(), out intVal);
                return !suc ? Enum.Parse(p_DestinationType, p_Inputvalue.ToString()) : p_Inputvalue;
            }

            #endregion

            #region Type

            if (p_DestinationType == typeof(Type))
            {
                return GetType(p_Inputvalue.ToString());
            }

            #endregion

            #region Nullable

            if (IsNullableType(p_DestinationType))
            {
                NullableConverter ncConverter = new NullableConverter(p_DestinationType);
                return ncConverter.ConvertFrom(p_Inputvalue);
            }

            #endregion

            //将double赋值给数值型的DataRow的字段是可以的，但是通过反射赋值给object的非double的其它数值类型的属性，却不行        
            return Convert.ChangeType(p_Inputvalue, p_DestinationType);
        }

        public static T ConvertType<T>(this object p_Inputvalue)
        {
            return (T) ConvertType(p_Inputvalue, typeof(T));
        }

        public static object ConvertType(this object p_Inputvalue, Type p_DestinationType)
        {
            object returnValue;
            if ((p_Inputvalue == null) || p_DestinationType.IsInstanceOfType(p_Inputvalue))
            {
                return p_Inputvalue;
            }

            var str = p_Inputvalue as string;
            if ((str != null) && (str.Length == 0))
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(p_DestinationType);
            var flag = converter.CanConvertFrom(p_Inputvalue.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(p_Inputvalue.GetType());
            }

            if (!flag && !converter.CanConvertTo(p_DestinationType))
            {
                throw new InvalidOperationException("无法转换成类型：" + p_Inputvalue + "==>" + p_DestinationType);
            }

            try
            {
                returnValue = flag
                    ? converter.ConvertFrom(null, null, p_Inputvalue)
                    : converter.ConvertTo(null, null, p_Inputvalue, p_DestinationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("类型转换出错：" + p_Inputvalue + "==>" + p_DestinationType, e);
            }

            return returnValue;
        }

        #endregion

        #region GetDefaultValue

        public static object GetDefaultValue(this Type p_DestType)
        {
            if (IsNumbericType(p_DestType))
            {
                return 0;
            }

            if (p_DestType == typeof(string))
            {
                return "";
            }

            if (p_DestType == typeof(bool))
            {
                return false;
            }

            if (p_DestType == typeof(DateTime))
            {
                return DateTime.Now;
            }

            if (p_DestType == typeof(Guid))
            {
                return Guid.NewGuid();
            }

            if (p_DestType == typeof(TimeSpan))
            {
                return TimeSpan.Zero;
            }

            return null;
        }

        #endregion

        #region GetDefaultValueString

        public static string GetDefaultValueString(this Type p_DestType)
        {
            if (IsNumbericType(p_DestType))
            {
                return "0";
            }

            if (p_DestType == typeof(string))
            {
                return "\"\"";
            }

            if (p_DestType == typeof(bool))
            {
                return "false";
            }

            if (p_DestType == typeof(DateTime))
            {
                return "DateTime.Now";
            }

            if (p_DestType == typeof(Guid))
            {
                return "System.Guid.NewGuid()";
            }

            if (p_DestType == typeof(TimeSpan))
            {
                return "System.TimeSpan.Zero";
            }

            return "null";
        }

        #endregion

        #region GetTypeRegularName

        /// <summary>
        /// GetTypeRegularName 获取类型的完全名称，如"ESBasic.Filters.SourceFilter,ESBasic"
        /// </summary>      
        public static string GetTypeRegularName(this Type p_DestType)
        {
            var assName = p_DestType.Assembly.FullName.Split(',')[0];

            return string.Format("{0},{1}", p_DestType, assName);
        }

        public static string GetTypeRegularNameOf(this object p_Obj)
        {
            Type destType = p_Obj.GetType();
            return GetTypeRegularName(destType);
        }

        #endregion

        #region GetTypeByRegularName

        /// <summary>
        /// GetTypeByFullString 通过类型的完全名称获取类型，regularName如"ESBasic.Filters.SourceFilter,ESBasic"
        /// </summary>       
        public static Type GetTypeByRegularName(this string p_RegularName)
        {
            return GetType(p_RegularName);
        }

        #endregion

        #region GetType

        /// <summary>
        /// GetType  通过完全限定的类型名来加载对应的类型。typeAndAssName如"ESBasic.Filters.SourceFilter,ESBasic"。
        /// 如果为系统简单类型，则可以不带程序集名称。
        /// </summary>       
        public static Type GetType(string p_TypeAndAssName)
        {
            string[] names = p_TypeAndAssName.Split(',');
            if (names.Length < 2)
            {
                return Type.GetType(p_TypeAndAssName);
            }

            return GetType(names[0].Trim(), names[1].Trim());
        }

        /// <summary>
        /// GetType 加载assemblyName程序集中的名为typeFullName的类型。assemblyName不用带扩展名，如果目标类型在当前程序集中，assemblyName传入null	
        /// </summary>		
        public static Type GetType(string p_TypeFullName, string p_AssemblyName)
        {
            if (p_AssemblyName == null)
            {
                return Type.GetType(p_TypeFullName);
            }

            //搜索当前域中已加载的程序集
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in asses)
            {
                string[] names = ass.FullName.Split(',');
                if (names[0].Trim() == p_AssemblyName.Trim())
                {
                    return ass.GetType(p_TypeFullName);
                }
            }

            //加载目标程序集
            Assembly tarAssem = Assembly.Load(p_AssemblyName);
            if (tarAssem != null)
            {
                return tarAssem.GetType(p_TypeFullName);
            }

            return null;
        }

        #endregion

        #endregion

        /// <summary>
        /// 对象属性 设置和获取
        /// </summary>
        public static class Property
        {
            static readonly ConcurrentDictionary<string, Type> m_cacheTypes = new ConcurrentDictionary<string, Type>();

            static readonly ConcurrentDictionary<string, dynamic> m_cacheDelegates =
                new ConcurrentDictionary<string, dynamic>();

            private static dynamic SetDelegate<T>(PropertyInfo property)
            {
                var DeclaringExp = Expression.Parameter(typeof(T), "obj");
                var PropertyExp = Expression.Parameter(typeof(object), "val");
                var PropertyConvertExp = Expression.Convert(PropertyExp, property.PropertyType);
                var Exp = Expression.Call(DeclaringExp, property.GetSetMethod(), PropertyConvertExp);
                return Expression.Lambda<Action<T, dynamic>>(Exp, DeclaringExp, PropertyExp).Compile();
            }

            private static Func<T, dynamic> GetDelegate<T>(PropertyInfo property)
            {
                var DeclaringExp = Expression.Parameter(typeof(T), "obj");
                var PropertyExp = Expression.Property(DeclaringExp, property);
                var PropertyConvertExp = Expression.Convert(PropertyExp, typeof(object));
                return Expression.Lambda<Func<T, dynamic>>(PropertyConvertExp, DeclaringExp).Compile();
            }

            public static void SetValue<T>(T t, Expression<Func<T, dynamic>> expression, dynamic value) where T : class
            {
                if (expression != null && (expression.Body is MemberExpression | expression.Body is UnaryExpression))
                {
                    var propertyname = expression.Body is UnaryExpression
                        ? ((expression.Body as UnaryExpression).Operand as MemberExpression).Member.Name
                        : (expression.Body as MemberExpression).Member.Name;
                    SetValue(t, propertyname, value);
                }
                else
                {
                    throw new Exception("lambda表达式有误！");
                }
            }

            public static dynamic GetValue<T>(T t, Expression<Func<T, dynamic>> expression) where T : class
            {
                if (expression != null && (expression.Body is MemberExpression | expression.Body is UnaryExpression))
                {
                    var propertyname = expression.Body is UnaryExpression
                        ? ((expression.Body as UnaryExpression).Operand as MemberExpression).Member.Name
                        : (expression.Body as MemberExpression).Member.Name;
                    return GetValue(t, propertyname);
                }
                else
                {
                    throw new Exception("lambda表达式有误！");
                }
            }

            public static void SetValue<T>(T t, string propertyname, dynamic value) where T : class
            {
                string key = typeof(T).FullName + propertyname;
                if (!m_cacheDelegates.ContainsKey(key + "set"))
                {
                    PropertyInfo property = typeof(T).GetProperty(propertyname);
                    m_cacheDelegates[key + "set"] = SetDelegate<T>(property);
                    m_cacheTypes[key] = property.PropertyType;
                }

                if (value.GetType() == m_cacheTypes[key])
                    m_cacheDelegates[key + "set"](t, value);
                else
                    m_cacheDelegates[key + "set"](t, Convert.ChangeType(value, m_cacheTypes[key]));
            }

            public static dynamic GetValue<T>(T t, string propertyname) where T : class
            {
                string key = typeof(T).FullName + propertyname;
                if (!m_cacheDelegates.ContainsKey(key + "get"))
                {
                    PropertyInfo property = typeof(T).GetProperty(propertyname);
                    m_cacheDelegates[key + "get"] = GetDelegate<T>(property);
                }

                return m_cacheDelegates[key + "get"](t);
            }

            public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, dynamic>> expr) where T : class
            {
                if (expr.Body is MemberExpression | (expr.Body is UnaryExpression &&
                                                     ((UnaryExpression) expr.Body).Operand is MemberExpression))
                {
                    if ((expr.Body as MemberExpression).Expression is ParameterExpression)
                    {
                        return (expr.Body as MemberExpression).Member as System.Reflection.PropertyInfo;
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// 对象方法 快速调用和创建方法委托
        /// </summary>
        //public static class Method
        //{
        //    static readonly ConcurrentDictionary<string, MethodInvokeHandler> m_cacheHandlers = new ConcurrentDictionary<string, MethodInvokeHandler>();

        //    public static dynamic Invoker(dynamic target, MethodInfo methodinfo, params dynamic[] paramters)
        //    {
        //        if (methodinfo != null && methodinfo.DeclaringType != null)
        //        {
        //            if (methodinfo.IsStatic)
        //            {
        //                string key = methodinfo.DeclaringType.FullName + methodinfo.Name;
        //                MethodInvokeHandler invokeHandler;
        //                if (m_cacheHandlers.ContainsKey(key))
        //                {
        //                    invokeHandler = m_cacheHandlers[key];
        //                }
        //                else
        //                {
        //                    invokeHandler = GetMethodInvoker(methodinfo);
        //                    m_cacheHandlers[key] = invokeHandler;
        //                }
        //                return invokeHandler.Invoke(null, paramters);
        //            }
        //            else if (target != null)
        //            {
        //                string key = methodinfo.DeclaringType.FullName + methodinfo.Name;
        //                MethodInvokeHandler invokeHandler;
        //                if (m_cacheHandlers.ContainsKey(key))
        //                {
        //                    invokeHandler = m_cacheHandlers[key];
        //                }
        //                else
        //                {
        //                    invokeHandler = GetMethodInvoker(methodinfo);
        //                    m_cacheHandlers[key] = invokeHandler;
        //                }
        //                return invokeHandler.Invoke(target, paramters);
        //            }
        //        }
        //        return null;
        //    }

        //    public static dynamic Invoker(dynamic target, string methodname, params dynamic[] paramters)
        //    {
        //        if (target != null && !string.IsNullOrWhiteSpace(methodname))
        //        {
        //            string key = target.GetType().FullName + methodname;
        //            MethodInvokeHandler invokeHandler;
        //            if (m_cacheHandlers.ContainsKey(key))
        //            {
        //                invokeHandler = m_cacheHandlers[key];
        //            }
        //            else
        //            {
        //                var methodInfo = target.GetType().GetMethod(methodname);
        //                if (methodInfo != null)
        //                {
        //                    invokeHandler = GetMethodInvoker(methodInfo);
        //                    m_cacheHandlers[key] = invokeHandler;
        //                }
        //                else
        //                {
        //                    throw new Exception(string.Format("{0} does not have method : {1}", target.GetType().FullName, methodname));
        //                }
        //            }
        //            return invokeHandler.Invoke(target, paramters);

        //        }
        //        return null;
        //    }

        //    public static dynamic Invoker<T>(string methodname, params dynamic[] paramters)
        //    {
        //        Type type = typeof(T);
        //        string key = type.FullName + methodname;
        //        MethodInvokeHandler invokeHandler;
        //        if (m_cacheHandlers.ContainsKey(key))
        //        {
        //            invokeHandler = m_cacheHandlers[key];
        //        }
        //        else
        //        {
        //            var methodInfo = type.GetMethod(methodname);
        //            if (methodInfo != null && methodInfo.IsStatic)
        //            {
        //                invokeHandler = GetMethodInvoker(methodInfo);
        //                m_cacheHandlers[key] = invokeHandler;
        //            }
        //            else
        //            {
        //                throw new Exception(string.Format("{0} does not have static method : {1}", type.FullName, methodname));
        //            }
        //        }
        //        return invokeHandler.Invoke(null, paramters);
        //    }

        //    public static dynamic Invoker<T>(dynamic target, string methodname, params dynamic[] paramters)
        //    {
        //        Type type = typeof(T);
        //        string key = type.FullName + methodname;
        //        MethodInvokeHandler invokeHandler;
        //        if (m_cacheHandlers.ContainsKey(key))
        //        {
        //            invokeHandler = m_cacheHandlers[key];
        //        }
        //        else
        //        {
        //            var methodInfo = type.GetMethod(methodname);
        //            if (methodInfo != null && !methodInfo.IsStatic)
        //            {
        //                invokeHandler = GetMethodInvoker(methodInfo);
        //                m_cacheHandlers[key] = invokeHandler;
        //            }
        //            else
        //            {
        //                throw new Exception(string.Format("{0} does not have method : {1}", type.FullName, methodname));
        //            }
        //        }
        //        return invokeHandler.Invoke(target, paramters);
        //    }
        //}

        /// <summary>
        /// 自定义特性
        /// </summary>
        public static class Attribute
        {
            static readonly ConcurrentDictionary<string, dynamic> m_cacheCustomAttributes =
                new ConcurrentDictionary<string, dynamic>();

            /// <summary>
            /// 获取对象的自定义特性
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="p_Value"></param>
            /// <returns></returns>
            public static List<T> GetCustomAttribute<T>(dynamic p_Value) where T : System.Attribute
            {
                Type type = p_Value.GetType();
                string key = typeof(T).FullName + p_Value.ToString();
                if ((type.BaseType == typeof(MethodInfo) | type.BaseType == typeof(PropertyInfo) |
                     type.BaseType == typeof(FieldInfo)) && p_Value.DeclaringType != null)
                {
                    key += p_Value.DeclaringType.FullName;
                }

                if (!m_cacheCustomAttributes.ContainsKey(key))
                {
                    if (p_Value is Type)
                    {
                        object[] customattributes = ((Type) p_Value).GetCustomAttributes(typeof(T), true);
                        if (customattributes.Length > 0)
                        {
                            m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                        }
                    }
                    else
                    {
                        if (type.BaseType == typeof(Enum))
                        {
                            var fild = type.GetField(p_Value.ToString());
                            if (fild.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = fild.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(MethodInfo))
                        {
                            var method = ((MethodInfo) p_Value);
                            if (method.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = method.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(PropertyInfo))
                        {
                            var property = ((PropertyInfo) p_Value);
                            if (property.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = property.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(FieldInfo))
                        {
                            var fild = ((FieldInfo) p_Value);
                            if (fild.IsDefined(typeof(T), false))
                            {
                                object[] customattributes = fild.GetCustomAttributes(typeof(T), true);
                                if (customattributes.Length > 0)
                                {
                                    m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                                }
                            }
                        }
                        else if (type.BaseType == typeof(Object))
                        {
                            object[] customattributes = type.GetCustomAttributes(typeof(T), true);
                            if (customattributes.Length > 0)
                            {
                                m_cacheCustomAttributes[key] = customattributes.Select(p => (T) p).ToList();
                            }
                        }
                    }

                    if (!m_cacheCustomAttributes.ContainsKey(key))
                        m_cacheCustomAttributes[key] = new List<T>();
                }

                return m_cacheCustomAttributes[key];
            }
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace TinyStore.Site
{
    public class ApiResult
    {
        public enum ECode
        {
            [Description("执行失败")] Fail = 0,
            [Description("执行成功")] Success = 1,
            [Description("未知错误")] UnKonwError = 2,


            [Description("用户未登录")] UserOffLine = 10,
            [Description("验证失败")] AuthorizationFailed = 11,
            [Description("对象已存在")] TargetExist = 12,
            [Description("对象不存在")] TargetNotExist = 13,
            [Description("对象已变动")] TargetChanged = 14,
            [Description("数据格式错误")] DataFormatError = 15


            //[System.ComponentModel.Description("密码格式错误")]
            //PasswordFormatError = 50,
            //[System.ComponentModel.Description("邮件格式错误")]
            //EmailFormatError = 51,
            //[System.ComponentModel.Description("金额格式错误")]
            //AmountFormatError = 52,

            //[System.ComponentModel.Description("密码错误")]
            //PasswordError = 101,
            //[System.ComponentModel.Description("邮件码错误")]
            //EmailCodeError = 102,
            //[System.ComponentModel.Description("金额过小")]
            //AmountToLower = 105,
            //[System.ComponentModel.Description("金额过大")]
            //AmountToBig = 106,
            //[System.ComponentModel.Description("金额不相符")]
            //AmountNotEquals = 107,
            //[System.ComponentModel.Description("金额不足")]
            //AmountNotEnough = 108,
        }

        public ApiResult()
        {
            Code = ECode.Success;
        }

        public ApiResult(ECode code)
        {
            Code = code;
        }

        public ApiResult(string message, ECode code = ECode.Fail)
        {
            Code = code;
            Message = message;
        }

        public ECode Code { get; set; }

        public bool Result => Code == ECode.Success;

        public string Message { get; private set; }

        public static JsonResult RCode(ECode? code = null)
        {
            var result = code != null ? new ApiResult((ECode) code) : new ApiResult();
            return new JsonResult(result);
        }

        public static JsonResult RCode(string message = null, ECode code = ECode.Fail)
        {
            var result = message != null ? new ApiResult(message, code) : new ApiResult();
            return new JsonResult(result);
        }

        public static JsonResult RData<T>(T data)
        {
            var result = new ApiResult<T>(data);
            return new JsonResult(result);
        }
    }

    public class ApiResult<T> : ApiResult
    {
        public ApiResult()
        {
            Code = ECode.Success;
        }

        public ApiResult(T data)
        {
            Code = ECode.Success;
            Data = data;
        }

        public T Data { get; set; }
    }
    
    
    public class GridData<T>
    {
        public GridData(IList<T> p_rows, int p_total = 0, dynamic p_footer = null)
        {
            rows = p_rows;
            total = p_total == 0 ? p_rows.Count : p_total;
            footer = p_footer;
        }

        public IList<T> rows { get; private set; }
        public int total { get; private set; }
        public dynamic footer { get; private set; }

    }
}
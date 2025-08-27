using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MistCore.Data
{
    public enum OperationResultType
    {
        /// <summary>
        /// 执行成功
        /// </summary>
        [Description("执行成功")]
        Finished = 200,

        /// <summary>
        /// 无此内容
        /// </summary>
        [Description("无此内容")]
        NoContent = 204,

        /// <summary>
        /// 参数错误
        /// </summary>
        [Description("参数错误")]
        InvalidParamter = 400,

        /// <summary>
        /// 需先登录
        /// </summary>
        [Description("认证失败")]
        Unauthorized = 401,

        /// <summary>
        /// 无此权限
        /// </summary>
        [Description("无此权限")]
        Forbidden = 403,

        /// <summary>
        /// 未能找到
        /// </summary>
        [Description("未能找到")]
        NotFound = 404,

        /// <summary>
        /// 请求过于频繁
        /// </summary>
        [Description("请求限速")]
        TooManyRequests = 429,

        /// <summary>
        /// 需要验证码验证
        /// </summary>
        [Description("环境检测")]
        EnvCheck = 430,

        /// <summary>
        /// 请求异常
        /// </summary>
        [Description("请求异常")]
        Error = 500,
    }
}

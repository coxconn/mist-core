using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.EventBus
{
    /// <summary>
    /// 发布者
    /// 1、配置在方法上，方法一旦被调用，自动根据返回结果发布消息
    /// </summary>
    public class PublishAttribute : Attribute
    {
        public string RouteKey { get; set; }

        public PublishAttribute(string RouteKey)
        {
            this.RouteKey = RouteKey;
        }
    }
}

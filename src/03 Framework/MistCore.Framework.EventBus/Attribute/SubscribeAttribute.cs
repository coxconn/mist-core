using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.EventBus
{
    /// <summary>
    /// 订阅者
    /// 配置在Handler上，无需注册事件，自动Mapper. Mapper后事件自动生成或者是方法的传入参数。
    /// 此时的事件将不受Handler接口的影响
    /// </summary>
    public class SubscribeAttribute : Attribute
    {
        public string RouteKey { get; set; }

        public SubscribeAttribute(string RouteKey)
        {
            this.RouteKey = RouteKey;
        }
    }
}

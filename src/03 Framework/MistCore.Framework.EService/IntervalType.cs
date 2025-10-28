using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.EService
{
    public enum IntervalType
    {
        /// <summary>
        /// 左闭右闭区间 [a, b]
        /// 包含左右两个端点
        /// </summary>
        Closed,

        /// <summary>
        /// 左闭右开区间 [a, b)
        /// 包含左端点，不包含右端点
        /// </summary>
        LeftClosedRightOpen,

        /// <summary>
        /// 左开右闭区间 (a, b]
        /// 不包含左端点，包含右端点
        /// </summary>
        LeftOpenRightClosed,

        /// <summary>
        /// 左开右开区间 (a, b)
        /// 不包含左右两个端点
        /// </summary>
        Open
    }

}

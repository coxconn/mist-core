using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MistCore.Data
{
    public class PageInfo
    {
        /// <summary>
        /// Gets or sets the page number.
        /// 当前页码,从1开始，如果赋为-1，则表示查询不需要分页
        /// </summary>
        public virtual int PageNo { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// 每页记录数
        /// </summary>
        public virtual int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total item count.
        /// 总记录数
        /// </summary>
        public virtual long Total { get; set; }

        /// <summary>
        /// 排序 item [asc|desc][, item1 [asc|desc]...]
        /// </summary>
        public virtual string Order { get; set; }

        /// <summary>
        /// 通过索引检索
        /// </summary>
        public virtual int Skip { get; set; }

        /// <summary>
        /// 通过索引检索
        /// </summary>
        public virtual int Take { get; set; }

        /// <summary>
        /// Gets or sets the page count.
        /// 总页数
        /// </summary>
        public virtual int PageCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageInfo" /> class.
        /// </summary>
        public PageInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageInfo"/> class.
        /// </summary>
        /// <param name="PageNo">The page no.</param>
        /// <param name="PageSize">Size of the page.</param>
        /// <param name="Order">The order.</param>
        public PageInfo(int PageNo, int PageSize, string Order)
        {
            this.PageNo = PageNo;
            this.PageSize = PageSize;
            this.Order = Order;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(@"PageNo:{0} PageSize:{1} Total:{2} Order:{3};", PageNo, PageSize, Total, Order);
        }

    }
}

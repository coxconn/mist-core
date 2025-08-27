using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MistCore.Core.DTOMapper
{
    public class DTOMapToAttribute : DTOMapAttributeBase
    {
        public DTOMemberList MemberList { get; set; } = DTOMemberList.Source;

        public DTOMapToAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {

        }

        public DTOMapToAttribute(DTOMemberList memberList, params Type[] targetTypes)
            : this(targetTypes)
        {
            MemberList = memberList;
        }

    }
}
using System;

namespace MistCore.Core.DTOMapper
{
    public class DTOMapFromAttribute : DTOMapAttributeBase
    {
        public DTOMemberList MemberList { get; set; } = DTOMemberList.Destination;

        public DTOMapFromAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {

        }

        public DTOMapFromAttribute(DTOMemberList memberList, params Type[] targetTypes)
            : this(targetTypes)
        {
            MemberList = memberList;
        }

    }
}
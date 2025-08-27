using System;

namespace MistCore.Core.DTOMapper
{
    public class DTOMapAttribute : DTOMapAttributeBase
    {
        public DTOMapAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {

        }

    }
}
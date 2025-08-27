using System;

namespace MistCore.Core.DTOMapper
{
    public abstract class DTOMapAttributeBase : Attribute
    {
        public Type[] TargetTypes { get; private set; }

        protected DTOMapAttributeBase(params Type[] targetTypes)
        {
            TargetTypes = targetTypes;
        }

    }
}
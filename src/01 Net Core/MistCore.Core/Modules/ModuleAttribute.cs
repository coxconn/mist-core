using System;

namespace MistCore.Core.Modules
{
    public class ModuleAttribute : Attribute
    {
        public Type[] TargetTypes { get; private set; }
        public string ModuleName { get; private set; }

        public ModuleAttribute(params Type[] targetTypes)
        {
            TargetTypes = targetTypes;
        }

        public ModuleAttribute(string moduleName)
        {
            ModuleName = moduleName;
        }
    }

    public class ModuleAfterAttribute : ModuleAttribute
    {
        public ModuleAfterAttribute(params Type[] targetTypes) : base(targetTypes)
        {
        }

        public ModuleAfterAttribute(string moduleName) : base(moduleName)
        {
        }
    }

    public class ModuleBeforeAttribute : ModuleAttribute
    {
        public ModuleBeforeAttribute(params Type[] targetTypes) : base(targetTypes)
        {
        }

        public ModuleBeforeAttribute(string moduleName) : base(moduleName)
        {
        }
    }

}
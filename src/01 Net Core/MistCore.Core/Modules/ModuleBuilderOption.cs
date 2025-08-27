using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace MistCore.Core.Modules
{
    public class ModuleBuilderOption
    {
        internal readonly List<ModuleInfo> modules = new List<ModuleInfo>();

        public void AddModule(params ModuleInfo[] modules)
        {
            LoadModule(modules);
        }

        public ModuleBuilderOption AddModule(params Type[] types)
        {
            LoadModule(types);
            return this;
        }

        public ModuleBuilderOption AddModule<T>(params T[] types) where T: class, IModuleInitializer
        {
            LoadModule(types);
            return this;
        }

        public ModuleBuilderOption AddModule(params Assembly[] types)
        {
            LoadModule(types);
            return this;
        }

        public ModuleBuilderOption AddModule(string moduleName)
        {
            LoadModule(moduleName);
            return this;
        }

        #region Load Module
        private void LoadModule(params ModuleInfo[] modules)
        {
            modules = modules
                .Select(module =>
                {
                    if(module.Type == null)
                    {
                        module.Type = Type.GetType(module.Id);
                    }
                    return module;
                })
                .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t.Type) && t.Type != typeof(IModuleInitializer))
                .ToArray();

            LoadModules(modules);
        }

        private ModuleInfo[] LoadModule(params Type[] types)
        {
            var modules = (types??new Type[0])
                .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && t != typeof(IModuleInitializer))
                .Select(k => new ModuleInfo
                {
                    Id = k.AssemblyQualifiedName,
                    Name = k.FullName,
                    Type = k,
                }).ToArray();

            LoadModules(modules);
            return modules;
        }

        private void LoadModule<T>(params T[] types) where T : class, IModuleInitializer
        {
            var modules = types
                .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t.GetType()) && t.GetType() != typeof(IModuleInitializer))
                .Select(k => new ModuleInfo
                {
                    Id = k.GetType().AssemblyQualifiedName,
                    Name = k.GetType().FullName,
                    Type = k.GetType(),
                }).ToArray();

            LoadModules(modules);
        }

        private void LoadModule(params Assembly[] types)
        {
            //var modules = (types ?? new Assembly[0]).Where(c => c.GetTypes().Any(t => typeof(IModuleInitializer).IsAssignableFrom(t)))
            var modules = (types ?? new Assembly[0])
                .SelectMany(c => c.GetTypes())
                .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && t != typeof(IModuleInitializer))
                .Select(k => new ModuleInfo
                {
                    Id = k.AssemblyQualifiedName,
                    Name = k.FullName,
                    Type = k,
                }).ToArray();

            LoadModules(modules);
        }

        private ModuleInfo[] LoadModule(string moduleName)
        {
            var modules = ((moduleName ?? string.Empty).Split(';').Where(k => !string.IsNullOrWhiteSpace(k)))
                .Select(c=>Type.GetType(c))
                .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && t != typeof(IModuleInitializer))
                .Select(k => new ModuleInfo
                {
                    Id = k.AssemblyQualifiedName,
                    Name = k.FullName,
                    Type = k,
                }).ToArray();

            //modules = modules.Select(module =>
            //{
            //    if (module.Assembly == null)
            //    {
            //        module.Assembly = Assembly.Load(new AssemblyName(module.Id));
            //    }
            //    return module;
            //}).ToArray();

            LoadModules(modules);
            return modules;
        }

        #endregion

        #region Load Modules

        private void LoadModules(params ModuleInfo[] modules)
        {
            modules.ToList().ForEach(module =>
            {
                if (this.modules.Any(c => c.Id == module.Id))
                {
                    return;
                }

                if (module.Type == null)
                {
                    module.Type = Type.GetType(module.Id); //Assembly.Load(new AssemblyName(module.Id));
                }

                this.modules.Add(module);

                var moduleInitializerType = module.Type;

                if (moduleInitializerType.IsDefined(typeof(ModuleAttribute)))
                {
                    var attributes = moduleInitializerType.GetCustomAttributes<ModuleAttribute>();
                    foreach (var attribute in attributes)
                    {
                        var modules = new List<ModuleInfo>();
                        modules.AddRange(LoadModule(attribute.TargetTypes));
                        modules.AddRange(LoadModule(attribute.ModuleName));

                        var nmodules = modules.Where(c => this.modules.All(k => k.Id != c.Id)).ToList();
                        this.modules.AddRange(nmodules);
                    }
                }
                if (moduleInitializerType.IsDefined(typeof(ModuleBeforeAttribute)))
                {
                    var beforeModeuls = moduleInitializerType.GetCustomAttributes<ModuleBeforeAttribute>();
                    foreach (var attribute in beforeModeuls)
                    {
                        var modules = new List<ModuleInfo>();
                        modules.AddRange(LoadModule(attribute.TargetTypes));
                        modules.AddRange(LoadModule(attribute.ModuleName));

                        var index = this.modules.IndexOf(module);
                        var nmodules = modules.Where(c => this.modules.All(k => k.Id != c.Id)).ToList();
                        this.modules.InsertRange(index, nmodules);
                    }
                }
                if (moduleInitializerType.IsDefined(typeof(ModuleAfterAttribute)))
                {
                    var afterModeuls = moduleInitializerType.GetCustomAttributes<ModuleAfterAttribute>();
                    foreach (var attribute in afterModeuls)
                    {
                        var modules = new List<ModuleInfo>();
                        modules.AddRange(LoadModule(attribute.TargetTypes));
                        modules.AddRange(LoadModule(attribute.ModuleName));

                        var index = this.modules.IndexOf(module);
                        var nmodules = modules.Where(c => this.modules.All(k => k.Id != c.Id)).ToList();
                        this.modules.InsertRange(index + 1, nmodules);
                    }
                }

            });
        }

        #endregion

    }

}

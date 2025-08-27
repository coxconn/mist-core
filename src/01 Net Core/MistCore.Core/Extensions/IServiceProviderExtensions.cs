using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MistCore.Core;
using MistCore.Core.ConfigurationManager;
using MistCore.Core.Modules;
using MistCore.Core.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceProvider Extensions
    /// </summary>
    public static class IServiceProviderExtensions
    {
        public static void ServiceScopedLocator(this IServiceProvider applicationServices)
        {
            GlobalConfiguration.ServiceLocator = new ServiceScopedLocator(applicationServices);
        }
    }
}

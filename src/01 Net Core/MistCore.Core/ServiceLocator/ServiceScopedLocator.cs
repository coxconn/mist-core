using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MistCore.Core.ServiceLocator
{
    /// <summary>
    /// ServiceScopedLocator
    /// </summary>
    internal class ServiceScopedLocator : IServiceLocator
    {
        private IServiceProvider _serviceProvider;

        private const string PROVIDERKEY = "RequestServiceProvider";

        public ServiceScopedLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// GetInstance
        /// </summary>
        public IServiceProvider Instance
        {
            get
            {
                var serviceProvider = (IServiceProvider)CallContext.GetData(PROVIDERKEY);
                if (serviceProvider == null)
                {
                    serviceProvider = _serviceProvider.CreateScope().ServiceProvider;
                    CallContext.SetData(PROVIDERKEY, serviceProvider);
                }
                return serviceProvider;
            }
        }

    }
}

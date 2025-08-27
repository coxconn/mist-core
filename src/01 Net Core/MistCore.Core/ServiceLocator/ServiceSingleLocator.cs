using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Core.ServiceLocator
{
    /// <summary>
    /// ServiceSingleLocator
    /// </summary>
    internal class ServiceSingleLocator : IServiceLocator
    {
        private IServiceProvider _serviceProvider;

        public ServiceSingleLocator(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Instance
        /// </summary>
        public IServiceProvider Instance
        {
            get
            {
                return _serviceProvider;
            }
        }
    }
}

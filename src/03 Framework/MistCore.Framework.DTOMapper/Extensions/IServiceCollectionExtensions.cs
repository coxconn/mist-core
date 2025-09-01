using System;
using System.Linq;
using AutoMapper;
using MistCore.Core.DTOMapper;
using MistCore.Framework.DTOMapper;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {

        ///// <summary>
        ///// 加载模块
        ///// </summary>
        ///// <param name="services"></param>
        ///// <param name="configuration"></param>
        ///// <param name="env"></param>
        ///// <param name="option"></param>
        //public static IServiceCollection AddDTOMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction)
        //{
        //    if(services.All(c=>c.ServiceType != typeof(IDTOMapper)))
        //    {
        //        services.AddSingleton(typeof(IDTOMapper), typeof(DTOMapper));
        //    }

        //    return services.AddAutoMapper(configAction);
        //}

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <param name="option"></param>
        public static IServiceCollection AddDTOMapper(this IServiceCollection services, params Type[] types)
        {
            //if (services.All(c => c.ServiceType != typeof(IDTOMapper)))
            //{
            //    services.AddSingleton(typeof(IDTOMapper), typeof(DTOMapper));
            //}

            return services.AddAutoMapper(configAction => configAction.CreateAutoAttributeMaps(types));
        }


    }
}

using AutoMapper;
using MistCore.Core.DTOMapper;
using System;

namespace MistCore.Framework.DTOMapper
{
    public class DTOMapper : IDTOMapper
    {
        private IMapper mapper;

        public DTOMapper(IMapper mapper)
        {
            this.mapper = mapper;
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TDestination>(object source)
        {
            return mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return mapper.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return mapper.Map<TSource, TDestination>(source, destination);
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, Type sourceType, Type destinationType)
        {
            return mapper.Map(source, sourceType, destinationType);
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return mapper.Map(source, destinationType, sourceType, destinationType);
        }

    }
}
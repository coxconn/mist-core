using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MistCore.Core.Modules;

namespace MistCore.Framework.EService
{

    /// <summary>
    /// ESClient
    /// </summary>
    public partial class ESClient
    {

        #region Check Index Exist

        public async Task<bool> IndexExistAsync(string index)
        {
            return (await _client.IndexExistsAsync(index)).Exists;
        }
        #endregion

        #region Create Index
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <returns></returns>
        public async Task<ICreateIndexResponse> CreateIndexAsync(string index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            var response = await _client.CreateIndexAsync(index, selector);
            return response;
        }
        #endregion

        #region Create Map
        /// <summary>
        /// CreateMap
        /// </summary>
        /// <returns></returns>
        public async Task<IPutMappingResponse> CreateMapAsync<T>(Func<PutMappingDescriptor<T>, IPutMappingRequest> selector) where T : class
        {
            var response = await _client.MapAsync<T>(selector);
            return response;
        }

        #endregion

        /// <summary>
        /// 写入索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public async Task<IBulkResponse> BulkIndexAsync<T>(string index, string type, List<T> ts, bool log = false) where T : class
        {
            var query = new BulkDescriptor();
            var query1 = query.Index(index).Type(type).IndexMany(ts);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.BulkAsync(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IDeleteResponse> DeleteByIdAsync<T>(string index, string type, DocumentPath<T> id, bool log = false) where T : class
        {
            var rsp = await _client.DeleteAsync(id, d => d.Index(index).Type(type));

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task<IDeleteByQueryResponse> DeleteByQueryAsync<T>(string index, string type, Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector, bool log = false) where T : class
        {
            var query = new DeleteByQueryDescriptor<T>(index);
            query.Type(type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.DeleteByQueryAsync<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// ForceMerge
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IForceMergeResponse> ForceMergeAsync(Indices indices)
        {
            return await _client.ForceMergeAsync(indices, q => q.MaxNumSegments(1));
        }

        /// <summary>
        /// UpdateMany
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">索引</param>
        /// <param name="type">类型</param>
        /// <param name="ts">更新列表</param>
        /// <param name="fid">匹配主键</param>
        /// <param name="fdoc">更新字段</param>
        /// <returns></returns>
        public async Task<IBulkResponse> UpdateManyAsync<T>(string index, string type, List<T> ts, Func<T, Id> fid, Func<T, object> fdoc, bool log = false) where T : class
        {
            var query = new BulkDescriptor();
            var query1 = query.Index(index).Type(type).UpdateMany<T, object>(ts, (ud, t) => ud.Id(fid(t)).Doc(fdoc(t)));

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.BulkAsync(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<ISearchResponse<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector = null, bool log = false) where T : class
        {
            var query = new SearchDescriptor<T>();
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.SearchAsync<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

    }


    public partial class ESClient<T>
    {
        /// <summary>
        /// 检查索引
        /// </summary>
        public async Task CheckIndexAsync()
        {
            if (!await IndexExistAsync())
            {
                var createresponse = await CreateIndexAsync();
                if (!createresponse.IsValid)
                {
                    var def = new T();
                    logger.LogError($"failed on create index: {def.Index}", createresponse.OriginalException);
                    throw new Exception($"failed on create index: {def.Index}");
                }
                var mapresponse = await CreateMapAsync();
                if (!mapresponse.IsValid)
                {
                    var def = new T();
                    logger.LogError($"failed on create map: {def.Index}.{def.Type}", mapresponse.OriginalException);
                    throw new Exception($"failed on create index: {def.Index}");
                }
            }
        }

        public async Task<bool> IndexExistAsync()
        {
            var index = new T();
            return (await _client.IndexExistsAsync(index.Index)).Exists;
        }

        public async Task<ICreateIndexResponse> CreateIndexAsync()
        {
            var index = new T();
            var response = await _client.CreateIndexAsync(index.Index, cid => index.CreateIndex(cid));
            return response;
        }

        public async Task<IPutMappingResponse> CreateMapAsync()
        {
            var index = new T();
            var response = await _client.MapAsync<T>(m => index.Map(m.Index(index.Index).Type(index.Type)));
            return response;
        }

        public async Task<IBulkResponse> BulkIndexAsync(List<T> ts, bool log = false)
        {
            var index = new T();
            var query = new BulkDescriptor();
            var query1 = query.Index(index.Index).Type(index.Type).IndexMany(ts);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.BulkAsync(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public async Task<IDeleteResponse> DeleteByIdAsync(string id, bool log = false)
        {
            var index = new T();
            var rsp = await _client.DeleteAsync<T>(id, d => d.Index(index.Index).Type(index.Type));

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. {rsp.OriginalException}");

            return rsp;
        }

        public async Task<IDeleteByQueryResponse> DeleteByQueryAsync(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector, bool log = false)
        {
            var index = new T();
            var query = new DeleteByQueryDescriptor<T>(index.Index);
            query.Type(index.Type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.DeleteByQueryAsync<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public async Task<IForceMergeResponse> ForceMergeAsync()
        {
            var index = new T();
            //return _client.ForceMerge(Index, q => q.OnlyExpungeDeletes(true).MaxNumSegments(1));
            return await _client.ForceMergeAsync(index.Index, q => q.MaxNumSegments(1));
        }

        public async Task<IBulkResponse> UpdateManyAsync(List<T> ts, Func<T, Id> fid, Func<T, object> fdoc, bool log = false)
        {
            var index = new T();
            var query = new BulkDescriptor();
            var query1 = query.Index(index.Index).Type(index.Type).UpdateMany<T, object>(ts, (ud, t) => ud.Id(fid(t)).Doc(fdoc(t)));

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.BulkAsync(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public async Task<ISearchResponse<T>> SearchAsync(Func<SearchDescriptor<T>, ISearchRequest> selector, bool log = false)
        {
            var index = new T();
            var query = new SearchDescriptor<T>();
            query.Index(index.Index).Type(index.Type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = await _client.SearchAsync<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

    }



}

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
    public class ESClient
    {
        private readonly static ILogger<ESClient> logger = (ILogger<ESClient>)GlobalConfiguration.ServiceLocator.Instance.GetService(typeof(ILogger<ESClient>));

        private readonly static Dictionary<string, ElasticClient> _clients = new Dictionary<string, ElasticClient>();

        protected ElasticClient _client;

        private string _conn;
        public string conn { get { return _conn; } }

        public ESClient(string conn)
        {
            this._conn = conn;

            _client = GetElasticClient(conn);
        }

        public ElasticClient GetElasticClient(string conn)
        {
            ElasticClient model = null;

            lock (_clients)
            {
                if (_clients.TryGetValue(conn, out model))
                {
                    return model;
                }

                var uris = conn.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).Select(u => new Uri(u));
                StaticConnectionPool pool = new StaticConnectionPool(uris);
                ConnectionSettings setting = new ConnectionSettings(pool);
#if DEBUG
                setting.DisableDirectStreaming(true);
#endif
                _client = new ElasticClient(setting);

                lock (_clients)
                {
                    _clients[conn] = _client;
                    return _client;
                }
            }
        }

        #region Check Index Exist

        public bool IndexExist(string index)
        {
            return _client.IndexExists(index).Exists;
        }
        #endregion

        #region Create Index
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <returns></returns>
        public ICreateIndexResponse CreateIndex(string index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            var response = _client.CreateIndex(index, selector);
            return response;
        }
        #endregion

        #region Create Map
        /// <summary>
        /// CreateMap
        /// </summary>
        /// <returns></returns>
        public IPutMappingResponse CreateMap<T>(Func<PutMappingDescriptor<T>, IPutMappingRequest> selector) where T : class
        {
            var response = _client.Map<T>(selector);
            return response;
        }

        #endregion

        /// <summary>
        /// 写入索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public IBulkResponse BulkIndex<T>(string index, string type, List<T> ts, bool log = false) where T : class
        {
            var query = new BulkDescriptor();
            var query1 = query.Index(index).Type(type).IndexMany(ts);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Bulk(b => query1);

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
        public IDeleteResponse DeleteById<T>(string index, string type, DocumentPath<T> id, bool log = false) where T : class
        {
            var rsp = _client.Delete(id, d => d.Index(index).Type(type));

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
        public IDeleteByQueryResponse DeleteByQuery<T>(string index, string type, Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector, bool log = false) where T : class
        {
            var query = new DeleteByQueryDescriptor<T>(index);
            query.Type(type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.DeleteByQuery<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// ForceMerge
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IForceMergeResponse ForceMerge(Indices indices)
        {
            return _client.ForceMerge(indices, q => q.MaxNumSegments(1));
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
        public IBulkResponse UpdateMany<T>(string index, string type, List<T> ts, Func<T, Id> fid, Func<T, object> fdoc, bool log = false) where T : class
        {
            var query = new BulkDescriptor();
            var query1 = query.Index(index).Type(type).UpdateMany<T, object>(ts, (ud, t) => ud.Id(fid(t)).Doc(fdoc(t)));

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Bulk(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public ISearchResponse<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector = null, bool log = false) where T : class
        {
            var query = new SearchDescriptor<T>();
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Search<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public string SerializeToString(object data, SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            return _client.Serializer.SerializeToString(data, formatting);
        }
    }


    public class ESClient<T> : ESClient where T : class, ESIndex<T>, new()
    {
        private readonly static ILogger<ESClient> logger = (ILogger<ESClient>)GlobalConfiguration.ServiceLocator.Instance.GetService(typeof(ILogger<ESClient>));

        public ESClient(string conn) : base(conn)
        {
            //var def = default(T);

            //#region 索引初始化逻辑
            //if (!IndexExist())
            //{
            //    if (!CreateIndex())
            //    {
            //        logger.Error($"failed on create index: {def.Index}");
            //        throw new Exception($"failed on create index: {def.Index}");
            //    }
            //    if (!CreateMap())
            //    {
            //        logger.Error($"failed on create map: {def.Index}.{def.Type}");
            //        throw new Exception($"failed on create index: {def.Index}");
            //    }
            //}
            //#endregion
        }

        /// <summary>
        /// 检查索引
        /// </summary>
        public void CheckIndex()
        {
            if (!IndexExist())
            {
                var createresponse = CreateIndex();
                if (!createresponse.IsValid)
                {
                    var def = new T();
                    logger.LogError($"failed on create index: {def.Index}", createresponse.OriginalException);
                    throw new Exception($"failed on create index: {def.Index}");
                }
                var mapresponse = CreateMap();
                if (!mapresponse.IsValid)
                {
                    var def = new T();
                    logger.LogError($"failed on create map: {def.Index}.{def.Type}", mapresponse.OriginalException);
                    throw new Exception($"failed on create index: {def.Index}");
                }
            }
        }

        public bool IndexExist()
        {
            var index = new T();
            return _client.IndexExists(index.Index).Exists;
        }

        public ICreateIndexResponse CreateIndex()
        {
            var index = new T();
            var response = _client.CreateIndex(index.Index, cid => index.CreateIndex(cid));
            return response;
        }

        public IPutMappingResponse CreateMap()
        {
            var index = new T();
            var response = _client.Map<T>(m => index.Map(m.Index(index.Index).Type(index.Type)));
            return response;
        }

        public IBulkResponse BulkIndex(List<T> ts, bool log = false)
        {
            var index = new T();
            var query = new BulkDescriptor();
            var query1 = query.Index(index.Index).Type(index.Type).IndexMany(ts);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Bulk(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public IDeleteResponse DeleteById(string id, bool log = false)
        {
            var index = new T();
            var rsp = _client.Delete<T>(id, d => d.Index(index.Index).Type(index.Type));

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. {rsp.OriginalException}");

            return rsp;
        }

        public IDeleteByQueryResponse DeleteByQuery(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector, bool log = false)
        {
            var index = new T();
            var query = new DeleteByQueryDescriptor<T>(index.Index);
            query.Type(index.Type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.DeleteByQuery<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public IForceMergeResponse ForceMerge()
        {
            var index = new T();
            //return _client.ForceMerge(Index, q => q.OnlyExpungeDeletes(true).MaxNumSegments(1));
            return _client.ForceMerge(index.Index, q => q.MaxNumSegments(1));
        }

        public IBulkResponse UpdateMany(List<T> ts, Func<T, Id> fid, Func<T, object> fdoc, bool log = false)
        {
            var index = new T();
            var query = new BulkDescriptor();
            var query1 = query.Index(index.Index).Type(index.Type).UpdateMany<T, object>(ts, (ud, t) => ud.Id(fid(t)).Doc(fdoc(t)));

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Bulk(b => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public ISearchResponse<T> Search(Func<SearchDescriptor<T>, ISearchRequest> selector, bool log = false)
        {
            var index = new T();
            var query = new SearchDescriptor<T>();
            query.Index(index.Index).Type(index.Type);
            var query1 = selector(query);

            var la = _client.Serializer.SerializeToString(query1, SerializationFormatting.None);
            var rsp = _client.Search<T>(s => query1);

            logger.Log(log ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Debug,
                $"url: {rsp.ApiCall.Uri.AbsoluteUri}. body: {la}. {rsp.OriginalException}");

            return rsp;
        }

        public string SerializeToString(SearchDescriptor<T> data, SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            return _client.Serializer.SerializeToString(data, formatting);
        }

    }



}

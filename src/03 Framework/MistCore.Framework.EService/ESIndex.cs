using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.EService
{
    public interface ESIndex<T> where T : class
    {
        IPutMappingRequest Map(PutMappingDescriptor<T> selector);

        ICreateIndexRequest CreateIndex(CreateIndexDescriptor cid, int shard = 5, int replica = 0);

        [JsonIgnore]
        string Index { get; }

        [JsonIgnore]
        string Type { get; }

    }

}

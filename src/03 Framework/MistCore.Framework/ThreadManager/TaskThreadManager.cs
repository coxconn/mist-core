using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MistCore.Core.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MistCore.Framework.ThreadManager
{
    public class TaskThreadManager<T> : IDisposable where T: class
    {

        private static readonly ILogger<TaskThreadManager<T>> logger = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<ILogger<TaskThreadManager<T>>>();

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        //当前任务处理了的数量
        private long parserCount = 0;
        //当批次处理数量
        private long batchCount = 0;
        /// <summary>
        /// 最大队列数量
        /// </summary>
        private int maxQueueCount;

        private CancellationTokenSource ctx;
        private bool isEnqueueComplete;
        private Task dequeueTask;
        private Task enqueueTask;

        public class EventRequestArgs : EventArgs
        {
            public EventRequestArgs(List<T> data)
            {
                this.data = data;
            }

            public EventRequestArgs(List<T> data, int index)
            {
                this.data = data;
                this.index = index;
            }

            public int index { get; }

            public List<T> data { get; }
        }
        public delegate void EventRequest(object sender, EventRequestArgs e);

        public event EventRequest OnEnqueue;
        public event EventRequest OnEnqueueGroup;
        public event EventRequest OnEnqueueComplete;
        public event EventRequest OnDequeue;
        public event EventRequest OnDequeueGroup;
        public event EventRequest OnComplete;

        /// <summary>
        /// 消费方法
        /// </summary>
        public Action<List<T>, CancellationTokenSource> DequeueAction; 
        /// <summary>
        /// 生产方法
        /// </summary>
        public Func<int, CancellationTokenSource, List<T>> EnqueueAction;

        public long GetParserCount()
        {
            return parserCount;
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="maxQueueCount">队列最大长度</param>
        /// <param name="dequeueThreadcount">消费最大线程数</param>
        /// <param name="dequeueBatchSize">消费当次处理数量</param>
        /// <param name="enqueueThreadcount">生产最大线程数</param>
        /// <param name="enqueueTotalCount">生产最大循环次数</param>
        public TaskThreadManager<T> Start(int maxQueueCount = 100000, int dequeueThreadcount = 1, int dequeueBatchSize = 1000, int enqueueThreadcount = 5, int enqueueTotalCount = 10000000)
        {
            if (this.ctx != null && !this.ctx.Token.IsCancellationRequested)
            {
                throw new ThreadStateException("Current is started.");
            }
            if (DequeueAction == null)
            {
                throw new ArgumentException("[DequeueAction] is not null.");
            }
            if (EnqueueAction == null)
            {
                throw new AggregateException("[EnqueueAction] is not null.");
            }

            this.maxQueueCount = maxQueueCount;
            this.ctx = new CancellationTokenSource();
            this.isEnqueueComplete = false;

            this.dequeueTask = Task.Run(() => TaskParser(DequeueAction, dequeueThreadcount, dequeueBatchSize));
            this.enqueueTask = Task.Run(() => TaskEnqueue(EnqueueAction, enqueueThreadcount, enqueueTotalCount));

            return this;
        }


        public void Dispose()
        {
            if (this.ctx != null && !this.ctx.IsCancellationRequested)
            {
                this.ctx.Cancel();
            }

            if(this.enqueueTask != null)
            {
                this.enqueueTask.Dispose();
                this.enqueueTask = null;
            }

            if(this.dequeueTask != null)
            {
                this.dequeueTask.Dispose();
                this.dequeueTask = null;
            }

            if(this.EnqueueAction != null)
            {
                var enqueueActionList = this.EnqueueAction.GetInvocationList();
                foreach (Func<int, CancellationTokenSource, List<T>> f in enqueueActionList)
                {
                    this.EnqueueAction -= f;
                }
            }

            if (this.DequeueAction != null)
            {
                var dequeueActionList = this.DequeueAction.GetInvocationList();
                foreach (Action<List<T>, CancellationTokenSource> f in dequeueActionList)
                {
                    this.DequeueAction -= f;
                }
            }


        }

        public void Wait()
        {
            if (this.enqueueTask != null)
            {
                this.enqueueTask.Wait();
            }

            if (this.dequeueTask != null)
            {
                this.dequeueTask.Wait();
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TaskParser(Action<List<T>, CancellationTokenSource> dequeueAction, int dequeueThreadcount, int dequeueBatchcount)
        {
            do
            {
                var currTmp = new List<T>();
                while (currTmp.Count < dequeueBatchcount * dequeueThreadcount)
                {
                    T t;
                    if (queue.Count > 0 && queue.TryDequeue(out t))
                    {
                        currTmp.Add(t);
                    }
                    else
                    {
                        break;
                    }
                }

                if (currTmp.Count > 0)
                {

                    var groups = currTmp
                        .Select((c, i) => new { group = i / dequeueBatchcount, detail = c }).GroupBy(c => c.group, (k, l) => l.Select(c => c.detail).ToList())
                        .ToList();

                    ParallelOptions options = new ParallelOptions();
                    options.MaxDegreeOfParallelism = dequeueThreadcount;
                    //options.CancellationToken = this.dequeuects.Token;

                    Parallel.ForEach(groups, options, (list, state)=> {
                        try
                        {
                            dequeueAction(list, ctx);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"{typeof(T).Name} parser error.");
                        }

                        logger.LogDebug($"dequeue count:{list.Count}.");
                        if (OnDequeue != null)
                        {
                            OnDequeue(this, new EventRequestArgs(list));
                        }
                    });

                    batchCount = currTmp.Count;
                    parserCount += batchCount;
                    logger.LogDebug($"OnDequeueGroup lastposition:{parserCount} lastupdatecount:{batchCount}");

                    if (OnDequeueGroup != null)
                    {
                        OnDequeueGroup(this, new EventRequestArgs(currTmp));
                    }

                    currTmp.Clear();
                }
                else if (isEnqueueComplete && queue.Count == 0) //完成
                {
                    logger.LogInformation($"OnComplete lastposition:{parserCount} lastupdatecount:{batchCount}");

                    if (OnComplete != null)
                    {
                        OnComplete(this, new EventRequestArgs(currTmp));
                    }

                    break;
                }
                else
                {
                    await Task.Delay(10000);
                }
            }
            while (!ctx.Token.IsCancellationRequested);

        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="enqueueFunc"></param>
        /// <param name="enqueueThreadcount"></param>
        /// <param name="enqueueTotalcount"></param>
        private void TaskEnqueue(Func<int, CancellationTokenSource, List<T>> enqueueFunc, int enqueueThreadcount, int enqueueTotalcount)
        {
            var operateCtx = new CancellationTokenSource();

            if (enqueueThreadcount <= 1)
            {
                for (var i = 0; i < enqueueTotalcount; i++)
                {
                    while (queue.Count > maxQueueCount)
                    {
                        if (operateCtx.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        if (ctx.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        logger.LogDebug($"Maximum number of queues reached. ({queue.Count}) / ({maxQueueCount})");
                        Task.Delay(5000);
                    }
                    if (operateCtx.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (ctx.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var list = enqueueFunc(i, operateCtx);
                    if (list != null && list.Count > 0)
                    {
                        logger.LogDebug($"enqueue count & group:{list.Count} index:{i}.");
                        if (OnEnqueue != null)
                        {
                            OnEnqueue(this, new EventRequestArgs(list, i));
                        }
                        if (OnEnqueueGroup != null)
                        {
                            OnEnqueueGroup(this, new EventRequestArgs(list, i));
                        }

                        list.ForEach(k =>
                        {
                            queue.Enqueue(k);
                        });
                    }
                    else
                    {
                        break; //没内容入队了，直接结束
                    }

                }
            }
            else
            {
                var currindex = 0;

                while (true)
                {
                    while (queue.Count > maxQueueCount)
                    {
                        if (operateCtx.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        if (ctx.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        logger.LogDebug($"Maximum number of queues reached. ({queue.Count}) / ({maxQueueCount})");
                        Task.Delay(5000);
                    }
                    if (operateCtx.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (ctx.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var listGroup = new int[enqueueThreadcount]
                        .AsParallel()
                        .Select((c, index) =>
                        {
                            var i = currindex + index;
                            var res = enqueueFunc(i, operateCtx);
                            return new { pageno = i, list = res };

                        })
                        .ToList();

                    var list = listGroup.SelectMany(c =>
                    {
                        if (c.list == null || c.list.Count == 0)
                        {
                            return new List<T>();
                        }
                        logger.LogDebug($"enqueue count:{c.list.Count} index:{c.pageno}.");
                        if (OnEnqueue != null)
                        {
                            OnEnqueue(this, new EventRequestArgs(c.list, c.pageno));
                        }
                        c.list.ForEach(k =>
                        {
                            queue.Enqueue(k);
                        });

                        return c.list;
                    }).ToList();

                    currindex += enqueueThreadcount;
                    if (list != null && list.Count > 0)
                    {
                        logger.LogDebug($"enqueue count group:{list.Count} index:{currindex -1}.");

                        if (OnEnqueueGroup != null)
                        {
                            OnEnqueueGroup(this, new EventRequestArgs(list, currindex -1));
                        }
                    }
                    else
                    {
                        break; //没内容入队了，直接结束
                    }

                    if (currindex >= enqueueTotalcount)
                    {
                        break;
                    }
                }

            }
            
            logger.LogInformation($"enqueue sync complete.");
            if (OnEnqueueComplete != null)
            {
                OnEnqueueComplete(this, new EventRequestArgs(null));
            }

            isEnqueueComplete = true;

        }


    }
}

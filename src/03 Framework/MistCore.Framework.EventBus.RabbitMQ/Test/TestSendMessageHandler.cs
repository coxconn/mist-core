using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus.Test
{
    public class TestSendMessageHandler : IEventHandler<string>
    {

        public async Task<bool> HandleEventAsync(string @event)
        {
            Console.WriteLine(@event);
            return true;
        }

    }
}

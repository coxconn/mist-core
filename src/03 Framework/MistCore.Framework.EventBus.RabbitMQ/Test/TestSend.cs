using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.EventBus.Test
{
    public class TestSend
    {
        private IEventBus eventBus;

        public TestSend(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }


        public void Test(string message, string sender)
        {
            this.eventBus.Trigger("message", message);
        }

    }
}

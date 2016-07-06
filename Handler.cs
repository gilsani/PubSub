using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
    internal class Handler
    {
        public WeakReference Subscriber { get; set; }
        public Delegate Action { get; set; }
    }
}

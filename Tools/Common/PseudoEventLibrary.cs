using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PseudoEventLibrary : IEventLibrary
    {
        private Dictionary<string, EventDescriptor> descLib;

        public PseudoEventLibrary(EventDescriptor[] descs)
        {
            this.descLib = new Dictionary<string, EventDescriptor>();
            foreach (EventDescriptor desc in descs)
                this.descLib.Add(desc.Name, desc);
        }

        public EventDescriptor GetDescriptor(string name)
        {
            return this.descLib[name];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    class Singleton
    {
        public Type ObjType { get; set; }

        public Type Interface { get; set; }

        private object _singletonInstance = null;

        private readonly object padlock = new object();

        private bool IsSet;

        public object SingletonInstance
        {
            get
            {
                lock (padlock)
                {
                    return _singletonInstance;
                }
            }
            set
            {
                if (value != null && !IsSet)
                {
                    IsSet = true;
                    _singletonInstance = value;
                }
            }
        }
    }
}

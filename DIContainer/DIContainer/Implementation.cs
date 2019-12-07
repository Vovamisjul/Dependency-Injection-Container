using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    class Implementation
    {
        public Implementation(Type implType, LifeTime lifetime)
        {
            ImplType = implType;
            Lifetime = lifetime;
        }
        public Type ImplType { get; }

        public object SingletonInstance { get; }
        public LifeTime Lifetime { get; }
    }
}

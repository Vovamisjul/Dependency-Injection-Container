using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer.Test.Data
{
    class Generic<TInterface> : IGeneric<TInterface> where TInterface : Interface1
    {
        public TInterface c()
        {
            throw new NotImplementedException();
        }
    }
}

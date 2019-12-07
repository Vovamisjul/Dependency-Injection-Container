using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer.Test.Data
{
    interface IGeneric<TInterface> where TInterface : Interface1
    {
        TInterface c();
    }
}

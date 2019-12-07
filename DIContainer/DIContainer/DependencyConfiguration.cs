using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    public class DependencyConfiguration
    {
        internal Dictionary<Type, List<Implementation>> Configuration { get; private set; }
        public DependencyConfiguration()
        {
            Configuration = new Dictionary<Type, List<Implementation>>();
        }
        public void Register(Type interfaceType, Type implementationType)
        {
            if (interfaceType.IsValueType || implementationType.IsValueType)
                throw new InvalidOperationException("Both implementation and interface should be reference types.");

            RegisterCore(interfaceType, implementationType, LifeTime.New);
        }
        
        public void RegisterSingleton(Type interfaceType, Type implementationType)
        {
            if (interfaceType.IsValueType || implementationType.IsValueType)
                throw new InvalidOperationException("Both implementation and interface should be reference types.");

            RegisterCore(interfaceType, implementationType, LifeTime.Singleton);
        }
        
        public void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            RegisterCore(typeof(TInterface), typeof(TImplementation), LifeTime.New);
        }
        
        public void RegisterSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            RegisterCore(typeof(TInterface), typeof(TImplementation), LifeTime.Singleton);
        }

        private void RegisterCore(Type interfaceType, Type implementationType, LifeTime lifetime)
        {
            if (implementationType.GetInterfaces().FirstOrDefault(x => x.Name == interfaceType.Name) == null && !interfaceType.Equals(implementationType))
                throw new InvalidOperationException($"Type {implementationType.ToString()} is not assignable from {interfaceType.ToString()}");

            if (implementationType.IsAbstract || implementationType.IsInterface)
                throw new InvalidOperationException($"Type {implementationType.ToString()} couldn't be abstract or interface.");

            if (Configuration.ContainsKey(interfaceType))
            {
                Configuration[interfaceType].Add(new Implementation(implementationType, lifetime));
            }
            else
            {
                Configuration.Add(interfaceType, new List<Implementation> { new Implementation(implementationType, lifetime) });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    public class DependencyProvider
    {
        private DependencyConfiguration _configuration;

        private Validator _validator;

        private List<Singleton> _singletons;

        public DependencyProvider(DependencyConfiguration config)
        {
            _configuration = config;
            _singletons = new List<Singleton>();
            _validator = new Validator(config.Configuration);
        }
        public TInterface Resolve<TInterface>()
        {
            return (TInterface)ResolveCore(typeof(TInterface));
        }

        public void ValidateConfig()
        {
            foreach (var entity in _configuration.Configuration)
            {
                foreach (var impl in entity.Value)
                {
                    _validator.Validate(impl.ImplType);
                }
            }
        }

        private object ResolveCore(Type interfaceType)
        {
            bool IsAllImplementationsRequested = false;

            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                //check if this is request for all implementations / only one 
                //i.e IEnumerable<T> => T implementations or IEnumerable<T> implementation

                if (!_configuration.Configuration.ContainsKey(interfaceType))
                {
                    interfaceType = interfaceType.GetGenericArguments()[0];
                    IsAllImplementationsRequested = true;
                }
            }
            List<Implementation> implementations;
            bool isOpenGenerics = false;
            if (_configuration.Configuration.ContainsKey(interfaceType))
            {
                implementations = _configuration.Configuration[interfaceType];
            }
            else
            {
                if (_configuration.Configuration.ContainsKey(interfaceType.GetGenericTypeDefinition()) && interfaceType.IsGenericType)
                {
                    implementations = _configuration.Configuration[interfaceType.GetGenericTypeDefinition()];
                    isOpenGenerics = true;
                }
                else
                {
                    throw new InvalidOperationException($"Dependency {interfaceType.ToString()} was not registered in container.");
                }
            }

            var result = new List<object>();


            foreach (var implementation in implementations)
            {
                result.Add(CreateObjectRecursive(implementation, interfaceType, isOpenGenerics));
            }

            if (IsAllImplementationsRequested)
                return ListObjToEnumerableType(result, interfaceType);
            else
                return result.First();
        }

        private object CreateObjectRecursive(Implementation implementation, Type interfaceType, bool IsOpenGenerics)
        {
            var constructorDependencies = GetConstructorDependencies(implementation.ImplType, interfaceType, IsOpenGenerics);

            if (constructorDependencies.Count() == 0)
                return CreateObjectCore(null, implementation, interfaceType, IsOpenGenerics);

            var parametersToPass = new object[constructorDependencies.Count()];

            for (int dependency = 0; dependency < constructorDependencies.Count(); ++dependency)
            {
                parametersToPass[dependency] = ResolveCore(constructorDependencies[dependency]);
            }

            return CreateObjectCore(parametersToPass, implementation, interfaceType, IsOpenGenerics);
        }

        private object CreateObjectCore(object[] constructorParams, Implementation implementation, Type interfaceType, bool IsOpenGenerics)
        {

            switch (implementation.Lifetime)
            {
                case LifeTime.New:
                {
                    if (IsOpenGenerics)
                        return CreateGenericObject(constructorParams, implementation.ImplType, interfaceType, implementation.Lifetime);

                    return CreateObjInternal(constructorParams, implementation.ImplType, interfaceType, implementation.Lifetime);

                }
                case LifeTime.Singleton:
                {
                    if (IsObjectCreated(implementation.ImplType))
                        return GetCreatedObject(implementation.ImplType);

                    if (IsOpenGenerics)
                        return CreateGenericObject(constructorParams, implementation.ImplType, interfaceType, implementation.Lifetime);

                    return CreateObjInternal(constructorParams, implementation.ImplType, interfaceType, implementation.Lifetime);
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }

        private Object CreateGenericObject(object[] constructorParams, Type implementationType, Type interfaceType, LifeTime lifetime)
        {
            //Set object type arguments to ones mentioned in container.
            return CreateObjInternal(constructorParams, implementationType.MakeGenericType(interfaceType.GenericTypeArguments), interfaceType, lifetime);
        }

        private Object CreateObjInternal(object[] constructorParams, Type implementationType, Type interfaceType, LifeTime lifetime)
        {
            object createdObj;

            if (constructorParams == null)
            {
                createdObj = Activator.CreateInstance(implementationType);
            }
            else
            {
                createdObj = GetConstructor(implementationType).Invoke(constructorParams);
            }

            if (lifetime == LifeTime.Singleton)
            {
                _singletons.Add(new Singleton()
                {
                    ObjType = implementationType,
                    Interface = interfaceType,
                    SingletonInstance = createdObj
                });
            }

            return createdObj;
        }

        private bool IsObjectCreated(Type t)
        {
            return _singletons.Find(x => x.ObjType == t) != null;
        }

        private Object GetCreatedObject(Type t)
        {
            return _singletons.Find(x => x.ObjType == t).SingletonInstance;
        }

        private List<Type> GetConstructorDependencies(Type classType, Type interfaceType, bool IsOpenGenerics)
        {
            var constructor = GetConstructor(classType);

            if (constructor == null)
                return null;

            var dependencies = constructor
            .GetParameters()
            .Select(x => x.ParameterType)
            .ToList();

            if (!IsOpenGenerics)
                return dependencies;

            var result = new List<Type>();

            foreach (var dependency in dependencies)
            {
                if (!dependency.IsGenericParameter)
                {
                    result.Add(dependency);
                    continue;
                }

                var resolvedArgs = interfaceType.GetGenericArguments();
                var genericArgs = interfaceType.GetGenericTypeDefinition().GetGenericArguments();
                int index = 0;

                if ((index = Array.FindIndex(genericArgs, x => x.Name == dependency.Name)) == -1)
                    throw new ArgumentException("Dependency in constructor was not present in the interface generic arguments.");

                var constraints = genericArgs[index].GetGenericParameterConstraints();

                result.Add(constraints[0]);
            }

            return result;
        }

        private ConstructorInfo GetConstructor(Type classType)
        {
            var constructors = classType.GetConstructors();
            return constructors.Length == 0 ? null : constructors[0];
        }
        private object ListObjToEnumerableType(List<object> items, Type type, bool performConversion = false)
        {
            var enumerableType = typeof(Enumerable);
            var castMethod = enumerableType.GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(type);
            var toListMethod = enumerableType.GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(type);

            IEnumerable<object> itemsToCast;

            if (performConversion)
            {
                itemsToCast = items.Select(item => Convert.ChangeType(item, type));
            }
            else
            {
                itemsToCast = items;
            }

            var castedItems = castMethod.Invoke(null, new[] { itemsToCast });

            return castedItems;
        }
    }
}

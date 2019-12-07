using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    class Validator
    {
        public Validator (Dictionary<Type, List<Implementation>> entities)
        {
            _entities = entities;
        }
        private Stack<Type> _dependencies = new Stack<Type>();
        private Dictionary<Type, List<Implementation>> _entities;
        public void Validate(Type newType)
        {
            if (ContainsCircularDependencies(newType))
                throw new ArgumentException($"Type {newType.ToString()} did contain circular dependencies.");

            var typeConstructors = newType.GetConstructors();

            if (typeConstructors.Length == 0)
                throw new ArgumentException($"{newType.ToString()} doesn't have constructors.");

            ParameterInfo[] constructorParams = typeConstructors[0].GetParameters();

            foreach (var param in constructorParams)
            {
                if (_entities.ContainsKey(param.ParameterType))
                {
                    var implementations = _entities[param.ParameterType];
                    _dependencies.Push(newType);
                    foreach (var implementation in implementations)
                        Validate(implementation.ImplType);
                    _dependencies.Pop();
                }
            }

        }

        private bool ContainsCircularDependencies(Type t)
        {
            foreach (var dependency in _dependencies)
            {
                if (dependency == t)
                    return true;
            }

            return false;
        }
    }
}

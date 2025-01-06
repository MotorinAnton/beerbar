using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;

namespace Core.Services
{
    public class SaveParametersService : IService
    {
        public readonly Dictionary<Type, IParameter> Parameters = typeof(IParameter)
            .Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IParameter).IsAssignableFrom(t))
            .ToDictionary(e => e, e => (IParameter)Activator.CreateInstance(e));

        public T Get<T>() where T : IParameter, new()
        {
            if (Parameters.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            throw new KeyNotFoundException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace Core.Services
{
    public partial class GameServices : SystemBase
    {
        private readonly Dictionary<Type, IService> _services = typeof(IService)
            .Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IService).IsAssignableFrom(t))
            .ToDictionary(e => e, e => (IService)Activator.CreateInstance(e));

        protected override void OnCreate()
        {
            var saveService = Get<SaveService>();
            saveService.Load();
            saveService.SaveData.SaveParameters.Initialize();
        }

        protected override void OnUpdate() { }

        public T Get<T>() where T : IService, new()
        {
            if (_services.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            throw new KeyNotFoundException();
        }
    }

    public interface IService { }
}
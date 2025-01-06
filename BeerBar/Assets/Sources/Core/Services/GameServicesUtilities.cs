using Unity.Entities;

namespace Core.Services
{
    public static class GameServicesUtilities
    {
        public static GameServices GetGameServices()
        {
            return World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameServices>();
        }

        public static T Get<T>() where T : IService, new()
        {
            return GetGameServices().Get<T>();
        }
    }
}
using MCLevelEdit.Interfaces;
using MCLevelEdit.Services;
using MCLevelEdit.ViewModels;
using Splat;

namespace MCLevelEdit
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<IFileService>(() => new FileService());
            services.RegisterLazySingleton<IMapService>(() => new MapService());
            services.RegisterLazySingleton<ITerrainService>(() => new TerrainService());
            services.RegisterLazySingleton(() => new MainViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>(), resolver.GetService<IFileService>()));
            services.RegisterLazySingleton(() => new EntitiesViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new MapViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new CreateEntityViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new CreateTerrainViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
        }
    }
}
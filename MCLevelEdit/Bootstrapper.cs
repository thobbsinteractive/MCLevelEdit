using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Infrastructure.Interfaces;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels;
using Splat;

namespace MCLevelEdit
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<IFilePort>(() => new FileAdapter());
            services.RegisterLazySingleton<ITerrainService>(() => new TerrainService());
            services.RegisterLazySingleton<IMapService>(() => new MapService(resolver.GetService<ITerrainService>(), resolver.GetService<IFilePort>()));
            services.RegisterLazySingleton(() => new MainViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new EntitiesTableViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new MapViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new CreateEntityViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.RegisterLazySingleton(() => new CreateTerrainViewModel(resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
        }
    }
}
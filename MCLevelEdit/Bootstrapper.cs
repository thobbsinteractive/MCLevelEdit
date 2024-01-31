using MCLevelEdit.Application.Model;
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
            services.RegisterLazySingleton(() => new EventAggregator<object>());
            services.RegisterLazySingleton<IFilePort>(() => new FileAdapter());
            services.RegisterLazySingleton<IPackagePort>(() => new MagicCarpetPackageAdapter());
            services.RegisterLazySingleton<ITerrainService>(() => new TerrainService());
            services.RegisterLazySingleton<IGameService>(() => new GameService(resolver.GetService<IPackagePort>()));
            services.RegisterLazySingleton<IMapService>(() => new MapService(resolver.GetService<EventAggregator<object>>(), resolver.GetService<ITerrainService>(), resolver.GetService<IFilePort>()));
            services.RegisterLazySingleton(() => new MainViewModel(resolver.GetService<EventAggregator<object>>(), resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.Register(() => new EntitiesTableViewModel(resolver.GetService<EventAggregator<object>>(), resolver.GetService<IMapService>(), resolver.GetService<ITerrainService>()));
            services.Register(() => new EditGameSettingsViewModel(resolver.GetService<EventAggregator<object>>(), resolver.GetService<IGameService>()));
        }
    }
}
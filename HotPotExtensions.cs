using Xmu.Crms.Services.HotPot;
using Xmu.Crms.Shared.Service;



namespace Microsoft.Extensions.DependencyInjection

{

    public static class HotPotExtensions

    {

        public static IServiceCollection AddHotPotClassService(this IServiceCollection serviceCollection) =>

            serviceCollection.AddScoped<IClassService, ClassService>();



    }

}
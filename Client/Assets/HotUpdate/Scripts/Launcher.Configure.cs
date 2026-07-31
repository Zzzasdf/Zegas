using System;
using Microsoft.Extensions.DependencyInjection;

partial class Launcher
{
    private class Configure
    {
        private ServiceCollection services = new ServiceCollection();
        
        public IServiceProvider Build()
        {
            return services.BuildServiceProvider();
        }
    }
}

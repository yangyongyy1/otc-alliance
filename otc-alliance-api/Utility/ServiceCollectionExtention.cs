using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class ServiceCollectionExtention
    {

        public static IServiceCollection AddNECaptchaValidateService(this IServiceCollection services)
        {
            services.AddScoped<NECaptchaValidateHelper>();
            return services;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MistCore.Core.Modules;
using MistCore.Framework.Cached;
using MistCore.Framework.Config;
using MistCore.Framework.Text;
using MistCore.Framework.Utility;

namespace MistCore.Framework
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {

        public void ConfigureServices(IServiceCollection services)
        {
            #region Cached
            services.AddSingleton(typeof(LocalCachePassivity));
            services.AddSingleton(typeof(ICache), typeof(LocalCachePassivity));

            #endregion

            #region Text
            services.AddSingleton(typeof(GZipCompression));
            services.AddSingleton(typeof(PinYinUtil));
            services.AddSingleton(typeof(PropertiesUtil));
            services.AddSingleton(typeof(RegexUtil));
            services.AddSingleton(typeof(TextEncryptor), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var encryptorInfo = new EncryptorInfo();
                configuration.GetSection("Encryptor").Bind(encryptorInfo);

                var textEncryptor = new TextEncryptor();
                return textEncryptor;
            });
            services.AddSingleton(typeof(TextStartUtil));
            services.AddSingleton(typeof(XmlUtil));
            #endregion

            #region Utility
            services.AddSingleton(typeof(GeoUtil));
            #endregion

        }

    }
}


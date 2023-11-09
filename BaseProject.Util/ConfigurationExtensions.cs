using Microsoft.Extensions.Configuration;

namespace BaseProject.Util
{
	public static class ConfigurationExtensions
    {

        public static T GetProperty<T>(this IConfiguration config, string section1, string section2, string prop)
        {
            var configSection1 = config.GetSection(section1);
            var configSection2 = configSection1.GetSection(section2);
            return configSection2.GetValue<T>(prop);
        }

        public static T GetProperty<T>(this IConfiguration config, string section, string prop)
        {
            var configSection = config.GetSection(section);
            return configSection.GetValue<T>(prop);
        }

        public static T GetProperty<T>(this IConfiguration config, string prop)
        {
            return config.GetValue<T>(prop);
        }


    }
}

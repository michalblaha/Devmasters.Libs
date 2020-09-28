namespace Devmasters
{
    public class Config
    {
        //#if NET47

        public static string GetWebConfigValue(string value)
        {
            string @out = System.Configuration.ConfigurationManager.AppSettings[value];
            if (@out == null)
            {
                @out = string.Empty;
            }
            return @out;
        }

        //#endif
    }


}

namespace TestProject.Common
{
    public class CommonFunction
    {
        IConfiguration _Configuration;
        IHostEnvironment _hstEnv;

        public CommonFunction(IConfiguration configuration,IHostEnvironment hstEnv)
        {
            _Configuration = configuration;
            _hstEnv = hstEnv;
        }

        public string GetConfigKey(string key)
        {
            try
            {
                string value = _Configuration.GetSection(key).Value;
                return value;
            }
            catch
            {
                return null;
            }
        }



    }
}

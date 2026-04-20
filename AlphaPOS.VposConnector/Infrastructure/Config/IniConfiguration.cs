using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaPOS.VposConnector.Infrastructure.Config
{
    public class IniConfiguration
    {
        private readonly string _path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        public IniConfiguration(string path)
        {
            _path = path;
        }

        public string Get(string section, string key)
        {
            try
            {
                var sb = new StringBuilder(1024);
                GetPrivateProfileString(section, key, "", sb, sb.Capacity, _path);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

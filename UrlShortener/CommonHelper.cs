#if !Core
using HandyControl.Data;
using System.Linq;
using System.Management;

namespace UrlShortener
{
    public class CommonHelper
    {
        public static SystemVersionInfo GetSystemVersionInfo()
        {
            ManagementClass managementClass = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection instances = managementClass.GetInstances();
            foreach (ManagementBaseObject instance in instances)
            {
                if (instance["Version"] is string version)
                {
                    System.Collections.Generic.List<int> nums = version.Split('.').Select(int.Parse).ToList();
                    SystemVersionInfo info = new SystemVersionInfo(nums[0], nums[1], nums[2]);
                    return info;
                }
            }
            return default(SystemVersionInfo);
        }
    }
}
#endif
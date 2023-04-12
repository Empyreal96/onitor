using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace MemUsageHelper
{
    class MemUsage
    {

        public static UInt64 GetCurrentMemUsage()
        {
            UInt64 appMemory = MemoryManager.AppMemoryUsage;
            return appMemory;

        }

        public static AppMemoryReport GetCurrentAppMemReport()
        {
            AppMemoryReport appMemReport = MemoryManager.GetAppMemoryReport();
            return appMemReport;
        }

        public static ProcessMemoryReport GetCurrentProcessMemReport()
        {
            ProcessMemoryReport processReport = MemoryManager.GetProcessMemoryReport();
            return processReport;
        }
    }
}

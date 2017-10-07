using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Utilities
{
    public static class ActionHelper
    {
        public static async Task TryUntilCompleteAsync(this Action action, int retry = 10, int interval = 3000)
        {
            while (retry-- > 0)
            {
                try
                {
                    action(); return;
                }
                catch
                {
                    await Task.Delay(interval);
                }
            }
        }

        public static async Task<T> TryUntilCompleteAsync<T>(this Func<T> func, int retry = 10, int interval = 3000)
        {
            while (retry-- > 0)
            {
                try
                {
                    return func();
                }
                catch
                {
                    await Task.Delay(interval);
                }
            }

            return default(T);
        }
    }
}

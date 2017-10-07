using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Omawari.Utilities
{
    public class ThrottoleAction
    {
        private DispatcherTimer timer { get; set; }

        public ThrottoleAction(Dispatcher dispatcher, Action action)
        {
            timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(300),
                DispatcherPriority.Normal,
                (sender, args) => { action(); timer.Stop(); },
                dispatcher);
        }

        public void Invoke()
        {
            timer.Stop();
            timer.Start();
        }
    }
}

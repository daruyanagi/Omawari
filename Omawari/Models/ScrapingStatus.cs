using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Models
{
    public enum ScrapingStatus
    {
        Pending,
        Running,
        Succeeded,
        Failed,
        Updated,
        Exception,
    }
}

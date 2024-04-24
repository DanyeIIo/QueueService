using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.BusinessLogic.Models
{
    public class QueueDataModel
    {
        public string RequestTime { get; set; }
        public string WriteTime { get; set; }
        public double ProcessingTime { get; set; }
    }
}

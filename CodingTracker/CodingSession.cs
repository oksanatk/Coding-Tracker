using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSCA.CodingTracker;
internal class CodingSession
{
    internal int id { get; set; }
    internal DateTime startTime { get; set; }
    internal DateTime endTime { get; set; }  
    internal int duration { get; set; }

    internal CodingSession(int id,  DateTime startTime, DateTime endTime, int duration)
    {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = duration;
    }

    internal CodingSession(DateTime startTime, DateTime endTime, int duration)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = duration;
    }
 /* DELete below and turn class into a record if no methods seem necessary.
  * 
 */


}
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
    internal DateTime duration { get; set; }

    internal CodingSession(int id,  DateTime startTime, DateTime endTime, DateTime duration)
    {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = duration;
    }

 /* DELete below and turn class into a record if no methods seem necessary.
  * 
  * 
  * 
    internal void StartSession() {}  //or should i make the StartSession just a constructor - b/c if i'm starting a session, then that's automatically a new session
    // so basically my app will start. then have the options to create a new coding session, view old coding sessions, update old coding session details

    internal void StopSession() {} 

    internal void UpdateSesssionDetails() {  }
 */


}
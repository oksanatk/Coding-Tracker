

namespace TSCA.CodingTracker;
internal class CodingSession
{
    internal int id { get; set; }
    internal DateTime startTime { get; private set; }
    internal DateTime endTime { get; private set; }  
    internal TimeSpan duration { get; private set; }

    internal System.Timers.Timer? timer;
    internal Action<TimeSpan>? TimerElapsed;

    internal CodingSession(int id,  DateTime startTime, DateTime endTime, TimeSpan duration)
    {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = duration;
    }

    internal CodingSession(DateTime startTime, DateTime endTime, TimeSpan duration)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = duration;
    }

    internal CodingSession(DateTime startTime)
    {
        this.startTime = startTime;
        this.endTime = new DateTime();
    }

    internal void StartTimer()
    {
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += OnTimerElapsed;
        timer.Start();
    }

    internal void StopTimer()
    {
        endTime = DateTime.Now;
        timer.Stop();
        timer.Dispose();
    }

    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        TimeSpan elapsedTime = DateTime.Now - startTime;
        duration = elapsedTime;
        TimerElapsed?.Invoke(elapsedTime);
    }
}
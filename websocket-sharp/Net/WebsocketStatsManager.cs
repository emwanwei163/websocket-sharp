using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace WebSocketSharp.Net
{
  internal class WebsocketStatsManager
  {

    private HashSet<string> _uniqueConnections;
    private DailyTimer _statsTimer;
    private Logger _log;

    public WebsocketStatsManager(Logger log) {
      _log = log;
      _uniqueConnections = new HashSet<string>();

      //Indicates that the timer will elapse at 00:00:00 of the next day (Midnight)
      _statsTimer = new DailyTimer(new TimeSpan(0));

      _statsTimer.Elapsed -= _statsTimer_Elapsed;
      _statsTimer.Elapsed += _statsTimer_Elapsed;
      _statsTimer.Start();
    }

    private void _statsTimer_Elapsed(object sender, EventArgs e)
    {
      if(_uniqueConnections != null) {
        _log.Trace($"Number of unique connections over the past 24 hours - {_uniqueConnections.Count}");
        _log.Trace($"Unique IP's connected over the past 24 hours - {string.Join(",", _uniqueConnections.ToArray())}");
      }
      _uniqueConnections.Clear();
    }

    public void AddUniqueConnection(string ipAddress){
      if(!string.IsNullOrEmpty(ipAddress)) {
        if(_uniqueConnections == null){
          _uniqueConnections = new HashSet<string>();
        }
        _uniqueConnections.Add(ipAddress);
      }
    }

    internal class DailyTimer
    {
      internal event EventHandler Elapsed = delegate { };
      private Timer timer;
      private DateTime previousRun;
      private TimeSpan elapseTimeSpan;

      internal DailyTimer(TimeSpan elapseTime)
      {
        timer = new Timer();
        timer.AutoReset = false;
        timer.Elapsed += ElapsedTimer;

        elapseTimeSpan = elapseTime;
      }

      internal void Start()
      {
        previousRun = DateTime.Today;

        TimeSpan timeSpanToElapsed = GetNextDay().Subtract(DateTime.Now);
        timer.Interval = timeSpanToElapsed.TotalMilliseconds;
        timer.Start();
      }

      private void ElapsedTimer(object sender, ElapsedEventArgs e)
      {
        if (previousRun != DateTime.Today)
          Elapsed(this, EventArgs.Empty);

        timer.Stop();
        Start();
      }

      private DateTime GetNextDay()
      {
        return DateTime.Today.AddDays(1).Add(elapseTimeSpan);
      }
    }




  }
}

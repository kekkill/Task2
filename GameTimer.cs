using System;
using System.Threading;

public class GameTimer
{
    private Timer _timer;
    private int _timeLimitMs;

    public bool TimeExpired;
    public Action OnTimeExpired;

    public GameTimer(int timeLimitMs)
    {
        _timeLimitMs = timeLimitMs;
    }

    public void Start()
    {
        TimeExpired = false;
        _timer = new Timer(TimerCallback, null, _timeLimitMs, Timeout.Infinite);
    }

    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Dispose();
            _timer = null;
        }
    }

    private void TimerCallback(object state)
    {
        TimeExpired = true;
        if (OnTimeExpired != null)
        {
            OnTimeExpired();
        }
    }
}
using UnityEngine;

public class Clock : MonoBehaviour
{
    Clock() { }

    //this is a singleton
    private static Clock instance = null;
    public static Clock Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<Clock>();
            }
            return instance;
        }
    }

    private void Update()
    {
        if (is_paused)
            return;

        accumulator += Time.deltaTime * timeScale * updatesPerSecond;
    }

    public int RequestAccumulatedUpdates()
    {
        int accumulatedUpdates = Mathf.FloorToInt(accumulator);
        accumulator -= accumulatedUpdates;

        return accumulatedUpdates;
    }

    public void UpdateElapsedTime()
    {
        elapsedTime++;
    }

    public void PauseClock(bool pause)
    {
        is_paused = pause;
    }
    public void SetTimeScale(int val = 1)
    {
        if (0 < val)
        {
            timeScale = val;
        }
    }
    public int TimeScale()
    {
        return timeScale;
    }
    public int ElapsedTime()
    {
        return elapsedTime;
    }
    public bool IsPaused()
    {
        return Instance.is_paused;
    }

    public int SecondsToMinutes(int seconds)
    {
        return seconds / 60;
    }
    public int MinutesToHours(int minutes)
    {
        return minutes / 60;
    }
    public int MinutesToDays(int minutes)
    {
        return minutes / 1440;
    }
    public int MinutesToSeconds(int minutes)
    {
        return minutes * 60;
    }
    public int HoursToMinutes(int hours)
    {
        return hours * 60;
    }
    public int DaysToMinutes(int days)
    {
        return days * 1440;
    }

    int elapsedTime = 0;
    //'time scale' or 'ticks per second'
    int timeScale = 1;
    int updatesPerSecond = 1;
    bool is_paused = false;
    float accumulator = 0;
}

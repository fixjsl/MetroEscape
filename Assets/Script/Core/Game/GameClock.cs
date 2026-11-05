using System;

public class GameClock
{
    bool running;
    public int Day { get; private set; } = 1;  //진행 일
    public int Hour { get; private set; } = 0; // 시간
    public int Minute { get; private set; } = 0; //분

    public event Action<int, int> ChangeTime; // hour, minute
    public event Action<int, int> ChangeDay; // day, hour

    public void Restart() { Set(1, 0, 0); running = true; }
    public void Set(int day, int hour, int minute) { Day = day; Hour = hour; Minute = minute; }
    public void Stop() => running = false;
    public void Tick()
    {   // GameManager.ClockRoutine이 2.5초마다 부름
        if (running)
        {
            Minute += 1;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour += 1;
            }
            if (Hour >= 24)
            {
                Hour = 0;
                Day += 1;
                ChangeDay?.Invoke(Day, Hour);
            }
            ChangeTime?.Invoke(Hour, Minute);

        }
    }
}

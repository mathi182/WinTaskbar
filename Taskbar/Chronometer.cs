using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Taskbar
{
    public class Chronometer
    {
        private Stopwatch stopwatch;
        private DispatcherTimer timer;
        private ICollection<INotifiable> notifiables = new List<INotifiable>();

        public TimeSpan Elapsed { get { return stopwatch.Elapsed; } }
        public bool IsRunning { get; private set; } = false;

        public Chronometer() : this(TimeSpan.FromSeconds(1))
        {
            
        }

        public Chronometer(TimeSpan frequency)
        {
            stopwatch = new Stopwatch();
            timer = new DispatcherTimer();

            timer.Interval = frequency;
            timer.Tick += TimerTick;
        }

        public void AddNotifiable(INotifiable notifiable)
        {
            notifiables.Add(notifiable);
        }

        public void SetFrequency(TimeSpan timeSpan)
        {
            timer.Interval = timeSpan;
        }

        public void Start()
        {
            stopwatch.Start();
            timer.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            stopwatch.Stop();
            timer.Stop();
            IsRunning = false;
        }

        public void Reset()
        {
            stopwatch.Restart();
            timer.Stop();
            IsRunning = false;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            NotifyAll();
        }

        private void NotifyAll()
        {
            foreach (INotifiable notifiable in notifiables)
                notifiable.Notify();
        }
    }
}

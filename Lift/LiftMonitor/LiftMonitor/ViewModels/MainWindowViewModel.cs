using Avalonia.Threading;
using LiftMonitor.Models;
using ReactiveUI;
using System.Threading;
using System.Windows.Input;

namespace LiftMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _locker;

        private int _sleepingTime = 3000;

        private string _information;

        private Direction _direction;

        private Semaphore _semaphore;

        public string Information
        {
            get => _information;
            set => this.RaiseAndSetIfChanged(ref _information, value);
        }

        public ICommand UpCommand { get; }

        public ICommand DownCommand { get; }

        public MainWindowViewModel()
        {
            _locker = new();
            _information = "";
            _semaphore = new Semaphore(1, 5);
            UpCommand = ReactiveCommand.Create(Up);
            DownCommand = ReactiveCommand.Create(Down);
        }

        private void Up() => NewFloorRequest(Direction.Up);

        private void Down() => NewFloorRequest(Direction.Down);

        private void NewFloorRequest(Direction newDirection)
            => new Thread(CalculateNewFloor_Semaphore).Start(newDirection);

        private void CalculateNewFloor_Monitor(object? newDirectionObject)
        {
            if (newDirectionObject is Direction newDirection)
            {
                var threadName = Thread.CurrentThread.ManagedThreadId.ToString();

                Dispatcher.UIThread.Invoke(() =>
                {
                    Information += $"Thread {threadName} wont to enter in critical section (Monitor) newDirection : {newDirection}\n";
                });

                if (_direction == Direction.None || _direction == newDirection)
                {
                    lock (_locker)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            Information += $"Thread {threadName} in critical section (Monitor) newDirection : {newDirection}\n";
                        });

                        _direction = newDirection;
                        // we have a huge proccessing
                        Thread.Sleep(_sleepingTime);

                        _direction = Direction.None;

                        Dispatcher.UIThread.Invoke(() =>
                        {
                            Information += $"Thread {threadName} exit from critical section (Monitor) newDirection : {newDirection}\n";
                        });
                    }
                }
            }
        }

        private void CalculateNewFloor_Semaphore(object? newDirectionObject)
        {
            if (newDirectionObject is Direction newDirection)
            {
                var threadName = Thread.CurrentThread.ManagedThreadId.ToString();

                Dispatcher.UIThread.Invoke(() =>
                {
                    Information += $"Thread {threadName} wont to enter in critical section (Semaphore) newDirection : {newDirection}\n";
                });

                if (_direction == Direction.None || _direction == newDirection)
                {
                    _semaphore.WaitOne();

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Information += $"Thread {threadName} in critical section (Semaphore) newDirection : {newDirection}\n";
                    });

                    _direction = newDirection;
                    // we have a huge proccessing
                    Thread.Sleep(_sleepingTime);

                    _direction = Direction.None;
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Information += $"Thread {threadName} exit from critical section (Semaphore) newDirection : {newDirection}\n";
                    });

                    _semaphore.Release();
                }
            }
        }

    }
}

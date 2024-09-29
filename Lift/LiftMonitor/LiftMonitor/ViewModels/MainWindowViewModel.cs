using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading;

namespace LiftMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _locker;

        private int _currentFloor;

        private int _sleepingTime;

        private string _information;

        private Semaphore _semaphore;

        public int CurrentFloor
        {
            get => _currentFloor;
            set => this.RaiseAndSetIfChanged(ref _currentFloor, value);
        }

        public string Information
        {
            get => _information;
            set => this.RaiseAndSetIfChanged(ref _information, value);
        }

        public ReactiveCommand<int, Unit> CallLiftCommand { get; }

        public MainWindowViewModel()
        {
            _locker = new();
            _information = string.Empty;
            _sleepingTime = 750;
            _semaphore = new Semaphore(1, 1);
            CurrentFloor = 1;
            CallLiftCommand = ReactiveCommand.Create<int>(CallLift);
        }

        private void CallLift(int floor)
        {
            Information += $"Lift call to {floor}\n";
            NewFloorRequest(floor);
        }

        private void NewFloorRequest(int newFloor)
            //=> new Thread(CalculateNewFloor_Monitor).Start(newFloor);
            => new Thread(CalculateNewFloor_Semaphore).Start(newFloor);

        private void CalculateNewFloor_Monitor(object? obj)
        {
            if (obj is not int newFloor)
            {
                return;
            }

            var threadName = Environment.CurrentManagedThreadId.ToString();
            Dispatcher.UIThread.Invoke(() =>
            {
                Information += $"Thread {threadName} :: call floor {newFloor}\n";
            });

            lock (_locker)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Information += $"Thread {threadName} is entered in critical section (Monitor):: floor {newFloor}\n\n";
                });
                int flag = newFloor > CurrentFloor ? 1 : -1;

                for (; CurrentFloor != newFloor; CurrentFloor += flag)
                {
                    Thread.Sleep(_sleepingTime);
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    Information += $"Thread {threadName} is exit in critical section (Monitor):: floor {newFloor}\n\n";
                });
            }
        }

        private void CalculateNewFloor_Semaphore(object? obj)
        {
            if (obj is not int newFloor)
            {
                return;
            }

            var threadName = Environment.CurrentManagedThreadId.ToString();
            Dispatcher.UIThread.Invoke(() =>
            {
                Information += $"Thread {threadName} :: call floor {newFloor}\n";
            });


            _semaphore.WaitOne();
            int flag = newFloor > CurrentFloor ? 1 : -1;

            Dispatcher.UIThread.Invoke(() =>
            {
                Information += $"Thread {threadName} is entered in critical section (Semaphore):: floor {newFloor}\n\n";
            });

            for (; CurrentFloor != newFloor; CurrentFloor += flag)
            {
                Thread.Sleep(_sleepingTime);
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                Information += $"Thread {threadName} is exit in critical section (Semaphore):: floor {newFloor}\n\n";
            });
            _semaphore.Release();
        }
    }
}

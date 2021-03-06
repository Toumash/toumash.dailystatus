using DailyStatus.Common;
using DailyStatus.Common.Configuration;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using DailyStatus.Common.Model;
using System.Windows.Input;
using DailyStatus.UI.WpfExtensions;
using System.Windows.Controls;
using System.Collections.Generic;
using DailyStatus.UI.View;
using Onova;
using Onova.Services;
using System.Reflection;
using DailyStatus.Common.Exceptions;
using DailyStatus.Common.Services;

namespace DailyStatus.UI.ViewModel
{
    public class StatusViewModel : INotifyPropertyChanged
    {
        const int RefreshIntervalInSeconds = 5;

        bool firstSync = true;
        readonly TogglProxy _togglClient;
        DailyStatusConfiguration cfg;
        readonly DispatcherTimer _timer;

        TimeSpan _diff;
        DateTime? _lastUpdated;

        TimeSpan _todayHours;
        double _todayGaugeMaxValue = 8;
        ObservableCollection<Workspace> _workspaces = new ObservableCollection<Workspace>();

        public ObservableCollection<Workspace> Workspaces
        {
            get { return _workspaces; }
            set { _workspaces = value; NotifyPropertyChanged(nameof(Workspaces)); }
        }

        Workspace _selectedWorkspace = new Workspace();

        public Workspace SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set
            {
                if (SelectedWorkspace == value) return;
                _selectedWorkspace = value;
                cfg.WorkspaceId = _selectedWorkspace.Id;
                SaveSettings();
                NotifyPropertyChanged(nameof(SelectedWorkspace));
                ScheduleInstantRefresh();
            }
        }

        public void SaveSettings()
        {
            SettingsManager.SaveSettings(cfg);
        }

        // Returns string because we would like to configure the display type
        public string StatusString
        {
            get
            {
                if (cfg.DisplayType == DisplayType.Time)
                {
                    var sign = "-";
                    sign = Diff.TotalHours < 0 ? sign : "";
                    return $"{sign}{Math.Abs(Diff.Days * 24 + Diff.Hours):0}:{Math.Abs(Diff.Minutes):00}";
                }
                else
                {
                    return $"{(decimal)Diff.TotalHours * (cfg.HourRate):N}";
                }
            }
        }

        void ScheduleInstantRefresh()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(0);
        }

        public TimeSpan Diff
        {
            get { return _diff; }
            set
            {
                _diff = value;
                NotifyPropertyChanged(nameof(Diff));
                NotifyPropertyChanged(nameof(StatusString));
            }
        }
        public double TodayGaugeMaxValue
        {
            get => _todayGaugeMaxValue;
            set
            {
                _todayGaugeMaxValue = value;
                NotifyPropertyChanged(nameof(TodayGaugeMaxValue));
            }
        }

        public DateTime? LastUpdateTime
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                NotifyPropertyChanged(nameof(LastUpdateTime));
            }
        }

        TimeSpan TodayHours
        {
            get => _todayHours;
            set
            {
                _todayHours = value;
                NotifyPropertyChanged(nameof(TodaysCurrentWork));
            }
        }
        public double TodaysCurrentWork { get => Math.Min(TodayHours.TotalHours, _todayGaugeMaxValue); set { } }

        public ICommand CloseCommand
            => new RelayCommand(o => Environment.Exit(0));


        bool _offline;

        public bool OfflineMode
        {
            get => _offline;
            set
            {
                _offline = value;
                NotifyPropertyChanged(nameof(OfflineMode));
            }
        }
        bool _currentlyTracking;

        public bool IsTimerActive
        {
            get => _currentlyTracking;
            set
            {
                _currentlyTracking = value;
                NotifyPropertyChanged(nameof(IsTimerActive));
            }
        }

        public StatusViewModel()
        {
            Diff = TimeSpan.FromHours(2);
            LastUpdateTime = DateTime.Now;
            OfflineMode = false;
            IsTimerActive = true;
            TodayHours = TimeSpan.FromHours(2);
            Workspaces = new ObservableCollection<Workspace>()
                {
                    new Workspace() { Name ="."}
                };
            SelectedWorkspace = Workspaces.First();
        }

        public StatusViewModel(TogglProxy togglClient, DailyStatusConfiguration configuration, Window window)
        {
            _togglClient = togglClient;
            cfg = configuration;

            Init();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(RefreshIntervalInSeconds)
            };
            _timer.Tick += async (s, e) => await RefreshData();
            _timer.Start();
            ScheduleInstantRefresh();
            Window = window;
            CheckForUpdatesAsync();
        }

        private static void CheckForUpdatesAsync()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);
                // Configure to look for packages in specified directory and treat them as zips
                using (var manager = new UpdateManager(
                    new GithubPackageResolver("Toumash", "daily-status", "*.zip"),
                    new ZipPackageExtractor()))
                {
                    // Check for updates
                    var check = await manager.CheckForUpdatesAsync();
                    // If there are none, notify user and return
                    if (!check.CanUpdate)
                    {
                        return;
                    }

                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    var lastVersion = check.LastVersion.ToString();
                    if (MessageBox.Show($"Would you like to install an update? Current version: {currentVersion}. Available version: {lastVersion}", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // Prepare the latest update
                        await manager.PrepareUpdateAsync(check.LastVersion);

                        // Launch updater and exit
                        manager.LaunchUpdater(check.LastVersion);
                        Environment.Exit(0);
                    }
                }
            });
        }

        public void SelectWorkSpace(Workspace w)
        {
            SelectedWorkspace = w;
            NotifyPropertyChanged(nameof(ContextMenu));
            cfg.WorkspaceId = w.Id;
            SaveSettings();
        }

        public DisplayType DisplayType
        {
            get
            {
                return cfg.DisplayType;
            }
            set
            {
                cfg.DisplayType = value;
                NotifyPropertyChanged(nameof(StatusString));
                NotifyPropertyChanged(nameof(ContextMenu));
                SaveSettings();
            }
        }

        public decimal HourRate
        {
            get
            {
                return cfg.HourRate;
            }
            set
            {
                cfg.HourRate = value;
                NotifyPropertyChanged(nameof(StatusString));
                NotifyPropertyChanged(nameof(ContextMenu));
                SaveSettings();
            }
        }

        public int HoursADay
        {
            get
            {
                return cfg.HoursADay;
            }
            set
            {
                cfg.HoursADay = value;
                NotifyPropertyChanged(nameof(StatusString));
                NotifyPropertyChanged(nameof(ContextMenu));
                SaveSettings();
            }
        }

        public DateTime SumSince
        {
            get
            {
                return cfg.SumSince;
            }
            set
            {
                cfg.SumSince = value;
                NotifyPropertyChanged(nameof(SumSince));
                NotifyPropertyChanged(nameof(ContextMenu));
                SaveSettings();
            }
        }

        public List<MenuItem> ContextMenu
        {
            get
            {
                var items = new List<MenuItem>();
                var workspaceItems = Workspaces.Select(w =>
                {
                    var item = new MenuItem() { Header = w.Name, Command = new RelayCommand(obj => SelectWorkSpace(w)) };
                    if (w == SelectedWorkspace)
                    {
                        item.Background = new SolidColorBrush(Colors.Green);
                    }
                    return item;
                });
                items.AddRange(workspaceItems);

                items.Add(new MenuItem()
                {
                    Header = "Display:" + DisplayType,
                    Command = new RelayCommand((_) =>
                    {
                        if (DisplayType == DisplayType.Money)
                            DisplayType = DisplayType.Time;
                        else DisplayType = DisplayType.Money;
                    }),
                    Background = new SolidColorBrush(Colors.DarkSlateBlue)
                });
                items.Add(new MenuItem()
                {
                    Header = "Hour rate: " + HourRate + "/h",
                    Command = new RelayCommand((_) =>
                    {
                        var prompt = new DecimalPrompt
                        {
                            Owner = this.Window,
                            Value = cfg.HourRate,
                            WindowTitle = "Hour rate",
                            WindowPrompt = "$$$ / hour: "
                        };

                        prompt.ShowDialog();
                        var newHourRate = prompt.Value;
                        HourRate = newHourRate;
                    })
                });
                items.Add(new MenuItem()
                {
                    Header = "Hours a day: " + cfg.HoursADay,
                    Command = new RelayCommand((_) =>
                    {
                        var prompt = new DecimalPrompt
                        {
                            Owner = this.Window,
                            Value = HoursADay,
                            WindowTitle = "Hours a day",
                            WindowPrompt = "hours / day: "
                        };

                        prompt.ShowDialog();
                        var newHoursADayValue = prompt.Value;
                        HoursADay = (int)newHoursADayValue;
                    })
                });
                items.Add(new MenuItem()
                {
                    Header = "Hour of start: " + cfg.WorkDayStartHour,
                    Command = new RelayCommand((_) =>
                    {
                        var prompt = new DecimalPrompt
                        {
                            Owner = this.Window,
                            Value = cfg.WorkDayStartHour,
                            WindowTitle = "Day starts at",
                            WindowPrompt = "Day start at x hour: "
                        };

                        prompt.ShowDialog();
                        var newDayStartHour = prompt.Value;
                        WorkDayStartHour = (int)newDayStartHour;
                    })
                });
                items.Add(new MenuItem()
                {
                    Header = "Sum since: " + cfg.SumSince.ToString("dd.MM.yyyy"),
                    Command = new RelayCommand((_) =>
                    {
                        var prompt = new DateTimeSincePrompt
                        {
                            Owner = this.Window,
                            Date = cfg.SumSince,
                            WindowTitle = "Sum hours since",
                            WindowPrompt = "Sum since: "
                        };

                        prompt.ShowDialog();
                        SumSince = prompt.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    })
                });
                items.Add(new MenuItem() { Header = DailyStatusBuildInfo.ShortVersion, Command = new RelayCommand((_) => { }) });
                items.Add(new MenuItem() { Header = "Minimize", Command = new RelayCommand((_) => WindowState = WindowState.Minimized) });
                items.Add(new MenuItem() { Header = "Close", Command = CloseCommand });
                return items;
            }
        }

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                _windowState = value;
                NotifyPropertyChanged(nameof(WindowState));
            }
        }

        public Window Window { get; }
        public int WorkDayStartHour
        {
            get { return cfg.WorkDayStartHour; }
            set
            {
                cfg.WorkDayStartHour = value;
                SaveSettings();
                this.NotifyPropertyChanged(nameof(WorkDayStartHour));
                this.NotifyPropertyChanged(nameof(ContextMenu));
            }
        }


        void Init()
        {
            Diff = TimeSpan.FromHours(0);
            LastUpdateTime = null;
            OfflineMode = false;
            TodayHours = TimeSpan.FromSeconds(0);
            Workspaces = new ObservableCollection<Workspace>(){
                new Workspace() { Name =  "Loading..." }
            };
            this._selectedWorkspace = Workspaces.First();
            _todayGaugeMaxValue = 8;
        }

        async Task RefreshData()
        {
            _timer.Interval = TimeSpan.FromSeconds(RefreshIntervalInSeconds);
            _timer.Stop();
            try
            {
                if (firstSync)
                {
                    Workspaces = new ObservableCollection<Workspace>(await _togglClient.GetAllWorkspaces());
                    SelectedWorkspace = Workspaces.FirstOrDefault(w => w.Id == cfg.WorkspaceId) ?? Workspaces.First();
                    NotifyPropertyChanged(nameof(ContextMenu));
                }
                _togglClient.SetWorkspace(SelectedWorkspace);
                var since = SumSince;
                var actual = await _togglClient.GetStatus(since);
                TodayHours = actual.TodaysHours;
                IsTimerActive = actual.IsTimerActive;

                var workday = new WorkDay(HoursADay, WorkDayStartHour);
                var expected = _togglClient.GetExpectedWorkingTime(workday, since, cfg.Holidays);
                Diff = _togglClient.GetDifference(expected: expected, sum: actual.TimeInMonth);
                LastUpdateTime = DateTime.Now;
                OfflineMode = false;
            }
            catch (OfflineException)
            {
                OfflineMode = true;
            }
            catch (BadRequestException)
            {
                // ignore, that is our exception and it happens occasionally
                // https://github.com/Toumash/daily-status/issues/19
            }
            finally
            {
                _timer.Start();
            }
            firstSync = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

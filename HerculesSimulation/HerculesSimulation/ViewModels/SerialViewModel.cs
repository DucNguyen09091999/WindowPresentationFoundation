using HerculesSimulation.Cores;
using HerculesSimulation.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows.Input;

namespace HerculesSimulation.ViewModels
{
    public class SerialViewModel : ViewModelBase
    {
        public ObservableCollection<string> PortNames { get; }
        public ObservableCollection<int> BaudRates { get; }
        public ObservableCollection<int> DataSizes { get; }
        public ObservableCollection<Parity> Parities { get; }
        public ObservableCollection<Handshake> Handshakes { get; }
        public ObservableCollection<string> Modes { get; } 

        private string _selectedPortName;
        public string SelectedPortName
        {
            get => _selectedPortName;
            set => SetProperty(ref _selectedPortName, value);
        }

        private int _selectedBaudRate;
        public int SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
        }

        private int _selectedDataSize;
        public int SelectedDataSize
        {
            get => _selectedDataSize;
            set => SetProperty(ref _selectedDataSize, value);
        }

        private Parity _selectedParity;
        public Parity SelectedParity
        {
            get => _selectedParity;
            set => SetProperty(ref _selectedParity, value);
        }

        private Handshake _selectedHandshake;
        public Handshake SelectedHandshake
        {
            get => _selectedHandshake;
            set => SetProperty(ref _selectedHandshake, value);
        }

        private string _selectedMode;
        public string SelectedMode
        {
            get => _selectedMode;
            set => SetProperty(ref _selectedMode, value);
        }

        private string _openButtonText = "Open";
        public string OpenButtonText
        {
            get => _openButtonText;
            set => SetProperty(ref _openButtonText, value);
        }

        // === Log ===
        public ObservableCollection<LogEntry> LogEntries { get; }

        // === Modem Lines ===
        public bool IsCdActive { get; set; } // Read-only
        public bool IsRiActive { get; set; } // Read-only
        public bool IsDsrActive { get; set; } // Read-only
        public bool IsCtsActive { get; set; } // Read-only
        public bool IsDtrEnabled { get; set; } = true; // Writable
        public bool IsRtsEnabled { get; set; } = true; // Writable

        // === Send ===
        public string SendText1 { get; set; }
        public bool IsHex1 { get; set; }
        public ICommand Send1Command { get; }

        public string SendText2 { get; set; }
        public bool IsHex2 { get; set; }
        public ICommand Send2Command { get; }

        public string SendText3 { get; set; }
        public bool IsHex3 { get; set; }
        public ICommand Send3Command { get; }

        // === Commands ===
        public ICommand OpenPortCommand { get; }
        public ICommand HwgFwUpdateCommand { get; }
        public string Version { get; set; } = "Version 3.2.8";


        public SerialViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
            PortNames = new ObservableCollection<string>(SerialPort.GetPortNames());
            BaudRates = new ObservableCollection<int> { 9600, 19200, 38400, 57600, 115200 };
            DataSizes = new ObservableCollection<int> { 8, 7 };
            Parities = new ObservableCollection<Parity>((Parity[])System.Enum.GetValues(typeof(Parity)));
            Handshakes = new ObservableCollection<Handshake>((Handshake[])System.Enum.GetValues(typeof(Handshake)));
            Modes = new ObservableCollection<string> { "Free", "NVT", "Telnet" };
            SelectedPortName = PortNames.FirstOrDefault();

            SelectedBaudRate = 9600;

            SelectedDataSize = 8;

            SelectedParity = Parity.None;

            SelectedHandshake = Handshake.None;

            SelectedMode = Modes.FirstOrDefault();

            OpenPortCommand = new RelayCommand(ExecuteOpenPort, CanExecuteOpenPort);
            Send1Command = new RelayCommand(p => ExecuteSend(SendText1, IsHex1));
            Send2Command = new RelayCommand(p => ExecuteSend(SendText2, IsHex2));
            Send3Command = new RelayCommand(p => ExecuteSend(SendText3, IsHex3));
        }

        private void ExecuteOpenPort(object obj)
        {
            if (OpenButtonText == "Open")
            {
                LogEntries.Add(new LogEntry($"Port {SelectedPortName} opened.", LogEntry.ColorStatus));
                OpenButtonText = "Close";
            }
            else
            {
                LogEntries.Add(new LogEntry("Port closed.", LogEntry.ColorError)); // Màu đỏ
                OpenButtonText = "Open";
            }
        }

        private bool CanExecuteOpenPort(object obj)
        {
            return !string.IsNullOrEmpty(SelectedPortName);
        }

        private void ExecuteSend(string text, bool isHex)
        {

            LogEntries.Add(new LogEntry($"[TX] -> {text}", LogEntry.ColorTx));
        }

        private void OnDataReceived(string data)
        {
            // Can dispatcher de cap nhat tu mot thread khac
            App.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(new LogEntry($"[RX] <- {data}", LogEntry.ColorRx));
            });
        }
    }
}

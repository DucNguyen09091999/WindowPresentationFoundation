using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HerculesSimulation.Models
{
    public class LogEntry
    {
        public string Timestamp { get; }
        public string Text { get; }

        // Luu mau sac duoi da
        public Brush Color { get; }

        public LogEntry(string text, Brush color)
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Text = text;
            Color = color;
        }

        // tao san mot so ham tinh de viewmodel goi
        public static Brush ColorStatus => Brushes.DarkGreen;
        public static Brush ColorError => Brushes.Red;
        public static Brush ColorTx => Brushes.Blue; // du lieu gui di (TX)
        public static Brush ColorRx => Brushes.Black; // du lieu nhan ve (RX)
    }
}

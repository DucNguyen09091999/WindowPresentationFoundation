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

        // Chúng ta sẽ lưu màu sắc dưới dạng Brush
        public Brush Color { get; }

        public LogEntry(string text, Brush color)
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Text = text;
            Color = color;
        }

        // Tạo sẵn một số màu tĩnh để ViewModel dễ gọi
        public static Brush ColorStatus => Brushes.DarkGreen;
        public static Brush ColorError => Brushes.Red;
        public static Brush ColorTx => Brushes.Blue; // Dữ liệu gửi đi (TX)
        public static Brush ColorRx => Brushes.Black; // Dữ liệu nhận (RX)
    }
}

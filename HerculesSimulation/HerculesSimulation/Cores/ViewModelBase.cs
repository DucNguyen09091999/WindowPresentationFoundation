using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HerculesSimulation.Cores
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //ham kiem tra xem gia tri co thuc su thay doi khong truoc khi thong bao
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            //khong co gi thay doi. Tra ve ngay lap tuc
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value; //gan gia tri moi
            OnPropertyChanged(propertyName); //thong bao cho view
            return true;
        }
    }
}

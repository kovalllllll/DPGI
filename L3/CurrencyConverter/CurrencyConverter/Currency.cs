using System.ComponentModel;

namespace CurrencyConverter.Models
{
    public class Currency : INotifyPropertyChanged
    {
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _flag = string.Empty;
        private double _rate;
        private int _units = 1;

        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged(nameof(Code));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Flag
        {
            get => _flag;
            set
            {
                _flag = value;
                OnPropertyChanged(nameof(Flag));
            }
        }

        /// <summary>Курс: скільки гривень за <see cref="Units"/> одиниць валюти</summary>
        public double Rate
        {
            get => _rate;
            set
            {
                _rate = value;
                OnPropertyChanged(nameof(Rate));
                OnPropertyChanged(nameof(RatePerUnit));
            }
        }

        /// <summary>Кількість одиниць, за яку зазначено курс (напр. 100 JPY)</summary>
        public int Units
        {
            get => _units;
            set
            {
                _units = value;
                OnPropertyChanged(nameof(Units));
                OnPropertyChanged(nameof(RatePerUnit));
            }
        }

        /// <summary>Курс за 1 одиницю валюти</summary>
        public double RatePerUnit => Units > 0 ? Rate / Units : 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
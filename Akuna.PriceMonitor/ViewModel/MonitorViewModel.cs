using Akuna.PriceMonitor.Model;
using Akuna.PriceMonitor.Command;
using Akuna.PriceService;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace Akuna.PriceMonitor.ViewModel
{
    internal sealed class MonitorViewModel : INotifyPropertyChanged
    {
        #region Fieds

        private const bool Bid = true;
        private const bool Ask = false;

        private readonly int _collectionSize = 10;
        private int _period = 1500000;

        private bool _isStarted;
        private bool _testModeActivated = false;
        private bool _orderSide = Bid;
        private string _selectedInstrument;
        private string _orderPrice;
        private string _orderQuantity;
        private ICommand _sendCommand;
        private RandomWalkPriceService _priceService;
        private TimeSpan _refreshTime;

        #endregion

        #region Properties

        public Instrument[] Instruments { get; set; }
        public string ObsPeriod { get { return $"{_period / 10000}ms"; } }
        public bool IsStarted
        {
            private get { return _isStarted; }
            set
            {
                _isStarted = value;
                if (_isStarted)
                    StartPriceService();
                else
                    StopPriceService();
                OnPropertyChanged(nameof(IsStarted));
            }
        }

        public int Period
        {
            get { return _period; }
            set
            {
                _period = value;
                UpdateRefreshPeriod();
                OnPropertyChanged(nameof(Period));
                OnPropertyChanged("ObsPeriod");
            }
        }

        public bool TestModeActivated
        {
            get { return _testModeActivated; }
            set
            {
                _testModeActivated = value;
                InitializeTestMode();
                OnPropertyChanged(nameof(TestModeActivated));
            }
        }

        public string SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                _selectedInstrument = value;
                OnPropertyChanged(nameof(SelectedInstrument));
            }
        }

        public bool OrderSide
        {
            get { return _orderSide; }
            set
            {
                _orderSide = value;
                OnPropertyChanged(nameof(OrderSide));
            }
        }

        public string OrderPrice
        {
            get { return _orderPrice; }
            set
            {
                _orderPrice = value;
                OnPropertyChanged(nameof(OrderPrice));
            }
        }

        public string OrderQuantity
        {
            get { return _orderQuantity; }
            set
            {
                _orderQuantity = value;
                OnPropertyChanged(nameof(OrderQuantity));
            }
        }

        public ICommand SendCommand
        {
            get
            {
                return _sendCommand;
            }
            set
            {
                _sendCommand = value;
            }
        }
        #endregion

        #region Constructor
        public MonitorViewModel()
        {
            SendCommand = new RelayCommand(new Action<object>(SendOrder));
            _refreshTime = new TimeSpan(Period);
            Application.Current.MainWindow.Closing += new CancelEventHandler(CloseApp);
            Instruments = new Instrument[_collectionSize];
            InitializeCollection();
            _priceService = new RandomWalkPriceService();
            _priceService.NewPricesArrived += PriceUpdateHandler;
        }
        #endregion

        public void UpdateRefreshPeriod()
        {
            _refreshTime = new TimeSpan(Period);
            for (int i = 0; i < _collectionSize; i++)
                Instruments[i].RefreshPeriod = _refreshTime;
        }

        private void CleanCollection()
        {
            for (int i = 0; i < _collectionSize; i++)
                Instruments[i].ResetData();
        }

        private void InitializeCollection()
        {
            for (int i = 0; i < _collectionSize; i++)
                Instruments[i] = new Instrument(i, _refreshTime);
        }

        private void InitializeTestMode()
        {
            StopPriceService();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                // Clean the values of the dataGrid 
                CleanCollection();
                // reset the background color of the dataGrid
                CleanCollection();
            });
        }

        internal void StartPriceService()
        {
            if (!TestModeActivated)
                _priceService.Start();
        }

        internal void StopPriceService()
        {
            if (_priceService.IsStarted)
                _priceService.Stop();
        }

        internal void CloseApp(object sender, CancelEventArgs e)
        {
            StopPriceService();
            Application.Current.Dispatcher.InvokeShutdown();
        }

        private void PriceUpdateHandler(IPriceService sender, uint instrumentID, IPrices prices)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                Instruments[(int)instrumentID].UpdatePrices(prices);
            });
        }

        public void SendOrder(object obj)
        {
            if (string.IsNullOrEmpty(SelectedInstrument))
            {
                MessageBox.Show($"Please define a Instrument Nb from: 0 to: {_collectionSize}");
                return;
            }
            int instrumentID;
            if (!int.TryParse(SelectedInstrument, out instrumentID) || instrumentID > _collectionSize - 1)
            {
                MessageBox.Show($"Instrument nb must be a number inferior to: {_collectionSize}");
                return;
            }

            double price;
            if (string.IsNullOrEmpty(OrderPrice) || !double.TryParse(OrderPrice, out price))
            {
                MessageBox.Show("Price must be a number");
                return;
            }

            int quantity;
            if (string.IsNullOrEmpty(OrderQuantity) || !int.TryParse(OrderQuantity, out quantity))
            {
                MessageBox.Show("Quantity must be an integer");
                return;
            }

            Order myOrder = new Order(price, quantity, OrderSide);

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                Instruments[instrumentID].UpdatePrices(myOrder);
            });
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}

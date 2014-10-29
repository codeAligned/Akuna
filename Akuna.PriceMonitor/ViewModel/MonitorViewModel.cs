using Akuna.PriceMonitor.Model;
using Akuna.PriceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Akuna.PriceMonitor.ViewModel
{
    public class MonitorViewModel : INotifyPropertyChanged
    {
        #region Parameters

        private const bool bid = true;
        private const bool ask = false;

        private int collectionSize = 10;
        private RandomWalkPriceService priceService;
        private int period = 1500000;
        public string ObsPeriod { get { return (period / 10000) + "ms"; } }
        private TimeSpan refreshTime;
        public Instrument[] Instruments { get; set; }
        bool isStarted;
        bool testModeActivated = false;
        string selectedInstrument;
        bool orderSide = bid;
        string orderPrice;
        string orderQuantity;
        private ICommand sendCommand;

        #endregion

        #region Assessors
        public bool IsStarted
        {
            private get { return isStarted; }
            set
            {
                isStarted = value;
                if (isStarted)
                    StartPriceService();
                else
                    StopPriceService();
                OnPropertyChanged("IsStarted");
            }
        }

        public int Period
        {
            get { return period; }
            set
            {
                period = value;
                UpdateRefreshPeriod();
                OnPropertyChanged("Period");
                OnPropertyChanged("ObsPeriod");
            }
        }

        public bool TestModeActivated
        {
            get { return testModeActivated; }
            set
            {
                testModeActivated = value;
                InitializeTestMode();
                OnPropertyChanged("TestModeActivated");
            }
        }

        public string SelectedInstrument
        {
            get { return selectedInstrument; }
            set
            {
                selectedInstrument = value;
                OnPropertyChanged("SelectedInstrument");
            }
        }

        public bool OrderSide
        {
            get { return orderSide; }
            set
            {
                orderSide = value;
                OnPropertyChanged("OrderSide");
            }
        }

        public string OrderPrice
        {
            get { return orderPrice; }
            set
            {
                orderPrice = value;
                OnPropertyChanged("OrderPrice");
            }
        }

        public string OrderQuantity
        {
            get { return orderQuantity; }
            set
            {
                orderQuantity = value;
                OnPropertyChanged("OrderQuantity");
            }
        }

        public ICommand SendCommand
        {
            get
            {
                return sendCommand;
            }
            set
            {
                sendCommand = value;
            }
        }
        #endregion

        #region Constructor
        public MonitorViewModel()
        {
            SendCommand = new RelayCommand(new Action<object>(SendOrder));
            refreshTime = new TimeSpan(Period);
            Application.Current.MainWindow.Closing += new CancelEventHandler(CloseApp);
            Instruments = new Instrument[collectionSize];
            InitializeCollection();
            priceService = new RandomWalkPriceService();
            priceService.NewPricesArrived += PriceUpdateHandler;
        }
        #endregion

        public void UpdateRefreshPeriod()
        {
            refreshTime = new TimeSpan(Period);
            for (int i = 0; i < collectionSize; i++)
                Instruments[i].RefreshPeriod = refreshTime;
        }

        private void CleanCollection()
        {
            for (int i = 0; i < collectionSize; i++)
                Instruments[i].ResetData();
        }

        private void InitializeCollection()
        {
            for (int i = 0; i < collectionSize; i++)
                Instruments[i] = new Instrument(i, refreshTime);
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
                priceService.Start();
        }

        internal void StopPriceService()
        {
            if (priceService.IsStarted)
                priceService.Stop();
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
                MessageBox.Show("Please define a Instrument Nb from: 0 to: " + collectionSize);
                return;
            }
            int instrumentID;
            if (!int.TryParse(SelectedInstrument, out instrumentID) || instrumentID > collectionSize - 1)
            {
                MessageBox.Show("Instrument nb must be a number inferior to: " + collectionSize);
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

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
            {
                Instruments[instrumentID].UpdatePrices(myOrder);
            });
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }

    class RelayCommand : ICommand
    {
        private Action<object> _action;

        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        #endregion
    }
}

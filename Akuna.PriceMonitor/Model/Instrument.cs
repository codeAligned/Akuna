using Akuna.PriceService;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Akuna.PriceMonitor.Model
{
    internal sealed class Instrument : IPrices, INotifyPropertyChanged
    {
        #region Fields

        private string _instrumentId;
        private double _bidPx;
        private uint _bidQty;
        private double _askPx;
        private uint _askQty;
        private uint _volume;
        private Stopwatch _stopwatch;

        #endregion

        #region Properties

        public TimeSpan RefreshPeriod { get; set; }
        public int DeltaBidPx { get; set; }
        public int DeltaAskPx { get; set; }

        public string ObsBidPx { get { return BidPx.ToString("N2"); } }
        public string ObsAskPx { get { return AskPx.ToString("N2"); } }
        public string ObsBidQty { get { return BidQty.ToString("N0"); } }
        public string ObsAskQty { get { return AskQty.ToString("N0"); } }
        public string ObsVolume { get { return Volume.ToString("N0"); } }

        public string InstrumentID
        {
            get { return _instrumentId; }
            set
            {
                _instrumentId = value;
                OnPropertyChanged("InstrumentID");
            }
        }

        public double BidPx
        {
            get { return _bidPx; }
            set
            {
                _bidPx = value;
                OnPropertyChanged("ObsBidPx");
                OnPropertyChanged("DeltaBidPx");
            }
        }

        public uint BidQty
        {
            get { return _bidQty; }
            set
            {
                _bidQty = value;
                OnPropertyChanged("ObsBidQty");
            }
        }

        public double AskPx
        {
            get { return _askPx; }
            set
            {
                _askPx = value;
                OnPropertyChanged("ObsAskPx");
                OnPropertyChanged("DeltaAskPx");
            }
        }

        public uint AskQty
        {
            get { return _askQty; }
            set
            {
                _askQty = value;
                OnPropertyChanged("ObsAskQty");
            }
        }

        public uint Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged("ObsVolume");
            }
        }
        #endregion

        #region Constructor
        public Instrument(int instruNb, TimeSpan refreshPeriod)
        {
            InstrumentID = "Instument " + instruNb;
            BidPx = 0;
            BidQty = 0;
            AskPx = 0;
            AskQty = 0;
            Volume = 0;

            _stopwatch = new Stopwatch();
            RefreshPeriod = refreshPeriod;
            _stopwatch.Start();
        }
        #endregion

        private int ComputeDelta(double holdValue, double newValue)
        {
            return (newValue > holdValue) ? 1 : (newValue < holdValue) ? -1 : 0;
        }

        public void UpdatePrices(IPrices newPrices)
        {
            if (_stopwatch.Elapsed > RefreshPeriod)
            {
                DeltaBidPx = ComputeDelta(BidPx, newPrices.BidPx);
                DeltaAskPx = ComputeDelta(AskPx, newPrices.AskPx);

                BidPx = newPrices.BidPx;
                BidQty = newPrices.BidQty;
                AskPx = newPrices.AskPx;
                AskQty = newPrices.AskQty;
                Volume = newPrices.Volume;

                _stopwatch.Restart();
            }
        }

        public void UpdatePrices(Order newOrder)
        {
            if (newOrder.Side == Order.SideType.ask)
            {
                DeltaAskPx = ComputeDelta(AskPx, newOrder.Price);
                AskPx = newOrder.Price;
                AskQty = (uint)newOrder.Quantity;
            }
            else if (newOrder.Side == Order.SideType.bid)
            {
                DeltaBidPx = ComputeDelta(BidPx, newOrder.Price);
                BidPx = newOrder.Price;
                BidQty = (uint)newOrder.Quantity;
            }

            Volume += (uint)newOrder.Quantity;
        }

        public void ResetData()
        {
            BidPx = 0;
            BidQty = 0;
            AskPx = 0;
            AskQty = 0;
            Volume = 0;
            DeltaAskPx = 0;
            DeltaBidPx = 0;
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

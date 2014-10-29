using Akuna.PriceService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Akuna.PriceMonitor.Model
{
    public class Instrument : IPrices, INotifyPropertyChanged
    {
        #region Parameters
        private string instrumentID;
        private double bidPx;
        private uint bidQty;
        private double askPx;
        private uint askQty;
        private uint volume;

        private Stopwatch stopwatch;
        internal TimeSpan RefreshPeriod { get; set; }

        public int DeltaBidPx { get; set; }
        public int DeltaAskPx { get; set; }
        #endregion

        #region Accessors

        public string ObsBidPx { get { return BidPx.ToString("N2"); } }
        public string ObsAskPx { get { return AskPx.ToString("N2"); } }
        public string ObsBidQty { get { return BidQty.ToString("N0"); } }
        public string ObsAskQty { get { return AskQty.ToString("N0"); } }
        public string ObsVolume { get { return Volume.ToString("N0"); } }

        public string InstrumentID
        {
            get { return instrumentID; }
            set
            {
                instrumentID = value;
                OnPropertyChanged("InstrumentID");
            }
        }


        public double BidPx
        {
            get { return bidPx; }
            set
            {
                bidPx = value;
                OnPropertyChanged("ObsBidPx");
                OnPropertyChanged("DeltaBidPx");
            }
        }

        public uint BidQty
        {
            get { return bidQty; }
            set
            {
                bidQty = value;
                OnPropertyChanged("ObsBidQty");
            }
        }

        public double AskPx
        {
            get { return askPx; }
            set
            {
                askPx = value;
                OnPropertyChanged("ObsAskPx");
                OnPropertyChanged("DeltaAskPx");
            }
        }

        public uint AskQty
        {
            get { return askQty; }
            set
            {
                askQty = value;
                OnPropertyChanged("ObsAskQty");
            }
        }

        public uint Volume
        {
            get { return volume; }
            set
            {
                volume = value;
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

            stopwatch = new Stopwatch();
            RefreshPeriod = refreshPeriod;
            stopwatch.Start();
        }
        #endregion

        private int ComputeDelta(double holdValue, double newValue)
        {
            return (newValue > holdValue) ? 1 : (newValue < holdValue) ? -1 : 0;
        }

        internal void UpdatePrices(IPrices newPrices)
        {
            if (stopwatch.Elapsed > RefreshPeriod)
            {
                DeltaBidPx = ComputeDelta(BidPx, newPrices.BidPx);
                DeltaAskPx = ComputeDelta(AskPx, newPrices.AskPx);

                BidPx = newPrices.BidPx;
                BidQty = newPrices.BidQty;
                AskPx = newPrices.AskPx;
                AskQty = newPrices.AskQty;
                Volume = newPrices.Volume;

                stopwatch.Restart();
            }
        }

        internal void UpdatePrices(Order newOrder)
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

        internal void ResetData()
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}

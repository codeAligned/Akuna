using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akuna.PriceMonitor.Model
{
    class Order
    {
        #region parameters
        const bool bid = true;
        const bool ask = false;
        public double Price { get; set; }
        public enum SideType { bid, ask };
        public SideType Side { get; set; }
        public int Quantity { get; set; }
        #endregion

        #region constructor
        public Order(double price,int quantity, bool orderSide )
        {
            Price = price;
            Quantity = quantity;

            Side = (orderSide == bid) ? SideType.bid : SideType.ask;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winkel
{
    public class OrderDetail
    {

        private int orderNumber;
        public int OrderNumber
        {
            get { return orderNumber; }
            set { orderNumber = value; }
        }

        private string productCode;

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }
        private int quantity;

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
        private double price;

        public double Price
        {
            get { return price; }
            set { price = value; }
        }
        private int orderLineNumber;

        public int OrderLineNumber
        {
            get { return orderLineNumber; }
            set { orderLineNumber = value; }
        }
    }
}

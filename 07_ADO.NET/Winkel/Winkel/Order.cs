// Lees aandachtig de getter van de instantievariabele Details.

namespace Winkel
{
    public class Order
    {
        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        private DateTime ordered;

        public DateTime Ordered
        {
            get { return ordered; }
            set { ordered = value; }
        }
        private DateTime required;

        public DateTime Required
        {
            get { return required; }
            set { required = value; }
        }
        private DateTime shipped;

        public DateTime Shipped
        {
            get { return shipped; }
            set { shipped = value; }
        }
        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private string comments;

        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        
        private int customerNumber;

        public int CustomerNumber
        {
            get { return customerNumber; }
            set { customerNumber = value; }
        }
        private List<OrderDetail> details;

        public List<OrderDetail> Details
        {
            get {
                if(details == null)
                {
                    details = new List<OrderDetail>();
                }    
                return details;
            }
            // geen setter, gebruik add op de getter
           
        }

        public override string ToString()
        {
            string detailinfo;
            if (details == null)
            {
                detailinfo = "*";
            }
            else
            {
                detailinfo = "" + details.Count;
            }
            return $"order nr {number} [op {ordered} besteld door klant {customerNumber}, {detailinfo} details]";
        }
    }
}

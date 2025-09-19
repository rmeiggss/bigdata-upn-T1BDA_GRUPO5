using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.MongoDb.Models
{
    public class OrderDetails : MongoDBbase
    {
        public long OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; } = 0;
        public short Quantity { get; set; } = 0;
        public float Discount { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }

    }
}

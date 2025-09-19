using DataAccess.SqlServer.Models;

namespace DataAccess.SqlServer.Readonly
{
    public class OrderDetailsReadonly : OrderDetail
    {
        public string ProductName { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;

    }
}

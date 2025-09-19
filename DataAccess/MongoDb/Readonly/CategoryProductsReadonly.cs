namespace DataAccess.MongoDb.Readonly
{
    public class CategoryProductsReadonly
    {
        public string CategoryName { get; set; } = null!;
        public List<ProductsGroupedReadonly> Products { get; set; } = new();
    }
    public class ProductsGroupedReadonly
    {
        public string ProductName { get; set; } = null!;
        public int TotalQuantity { get; set; }
    }
}

namespace LinQHomework.Controllers
{
    using LinQHomework.Data;
    using LinQHomework.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;


    public class LinQController : Controller
    {
        private readonly AppDbContext _context;


        private readonly string _connectionString;

        public LinQController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RetriveOrders()
        {
            var orders = from o in _context.Orders
                         select o;


            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var ordersList = await orders.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RetriveJoinedOrders()
        {
            var orders = from o in _context.Orders
                         join c in _context.Customers on o.CustomerId equals c.CustomerId
                         join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                         join p in _context.Products on oi.ProductId equals p.ProductId
                         select new
                         {
                             Order = o,
                             Customer = c,
                             OrderItem = oi,
                             Product = p
                         };


            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var ordersList = await orders.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOrderByProductId(int productId)
        {
            var orders = _context.Orders
                            .Include(o => o.OrderItems)
                            .Where(o => o.OrderItems.Any(oi => oi.ProductId == productId));


            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var ordersList = await orders.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RetrieveOrdersIncludeAllProps()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)  // Product can only be accessed through OrderItem
                .Include(o => o.Customer)

                // create customized return object:
                .Select(o => new // .Select(o => new Order  // Order's OrderItem is JsonIgnored
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Customer = o.Customer,
                    OrderItems = o.OrderItems.Select(oi => new OrderItem
                    {
                        OrderItemId = oi.OrderItemId,
                        OrderId = oi.OrderId,
                        //ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        Product = oi.Product    // object
                    }).ToList(),
                    Total = o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)
                });


            #region compiled sql:
            //            SELECT[o].[OrderId], [o].[OrderDate], [c].[CustomerId], [c].[Email], [c].[Name], [t].[OrderItemId], [t].[OrderId], [t].[ProductId], [t].[Quantity], [t].[ProductId0], [t].[Name], [t].[Price], (
            //    SELECT COALESCE(SUM(CAST([o1].[Quantity] AS decimal(18, 2)) * [p0].[Price]), 0.0)
            //    FROM[OrderItems] AS[o1]
            //    INNER JOIN[Products] AS[p0] ON[o1].[ProductId] = [p0].[ProductId]
            //    WHERE[o].[OrderId] = [o1].[OrderId])
            //FROM[Orders] AS[o]
            //INNER JOIN[Customers] AS[c] ON[o].[CustomerId] = [c].[CustomerId]
            //LEFT JOIN(
            //    SELECT[o0].[OrderItemId], [o0].[OrderId], [o0].[ProductId], [o0].[Quantity], [p].[ProductId] AS[ProductId0], [p].[Name], [p].[Price]
            //    FROM [OrderItems] AS [o0]
            //    INNER JOIN [Products] AS[p] ON [o0].[ProductId] = [p].[ProductId]
            //) AS[t] ON[o].[OrderId] = [t].[OrderId]
            //ORDER BY[o].[OrderId], [c].[CustomerId], [t].[OrderId], [t].[ProductId]
            #endregion
            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var ordersList = await orders.ToListAsync();
            return Ok(orders);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> RetrieveOrdersIncludeOrderItemsQuery()
        {
            // query syntax but still have to combine to use "Include()" method
            var orders = from o in _context.Orders
             .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
             .Include(o => o.Customer)
                         select new
                         {
                             Order = o,
                             Customer = o.Customer,
                             OrderItems = from oi in o.OrderItems
                                          select new
                                          {
                                              OrderItemId = oi.OrderItemId,
                                              OrderId = oi.OrderId,
                                              ProductId = oi.ProductId,
                                              Quantity = oi.Quantity,
                                              Product = oi.Product
                                          },
                             Total = o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)
                         };



            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var ordersList = await orders.ToListAsync();
            return Ok(orders);
        }

        // Get the total revenue generated by each customer for orders placed in the year 2022.
        [HttpGet("[action]")]
        public async Task<IActionResult> RevenueByEachCustomerReport()
        {
            // Query (Comprehension) Syntax, using the from, where, select, and group by keywords
            // LinQ provider will auto orderby descending by default
            // You can add an OrderByDescending clause to your LINQ query by chaining it after the GroupBy clause
            var query = from c in _context.Customers
                        join o in _context.Orders on c.CustomerId equals o.CustomerId
                        join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                        join p in _context.Products on oi.ProductId equals p.ProductId
                        where o.OrderDate.Year == 2022
                        group p.Price * oi.Quantity by c.Name into g
                        orderby g.Sum() descending // orderby here
                        select new { CustomerName = g.Key, Revenue = g.Sum() };
                        

            var sql = query.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var result = await query.ToListAsync();
            return Ok(result);
        }

        // calculates the total revenue generated by each product
        [HttpGet("[action]")]
        public async Task<IActionResult> RevenueByEachProduct()
        {
            // Lamda (Method) Syntax  (more concise)
            var query = _context.OrderItems
                .GroupBy(oi => oi.Product)
                .Select(g => new
                {
                    productName = g.Key.Name,
                    revenue = g.Sum(oi => oi.Quantity * oi.Product.Price)
                })
                .OrderByDescending(pr => pr.revenue);


            var sql = query.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var result = await query.ToListAsync();
            return Ok(result);
        }
        // calculates the total revenue generated by each product
        [HttpGet("[action]")]
        public async Task<IActionResult> RevenueByEachProductComprehension()
        {
            // Query (Comprehension) Syntax, using the from, where, select, and group by keywords
            var query = from oi in _context.OrderItems
                        group oi by oi.Product into g
                        orderby g.Sum(oi => oi.Quantity * oi.Product.Price) descending
                        select new
                        {
                            productName = g.Key.Name,
                            revenue = g.Sum(oi => oi.Quantity * oi.Product.Price)
                        };


            var sql = query.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var result = await query.ToListAsync();
            return Ok(result);
        }

        // retrieve a list of all orders that were made in a certain date range, sorted by order date,
        // along with the customer name and the total revenue of each order. 
        [HttpGet("[action]")]
        public async Task<IActionResult> OrdersOfADateRange()
        {
            DateTime startDate = new DateTime(2022, 1, 1);
            DateTime endDate = new DateTime(2022, 2, 1);

            var orders = from o in _context.Orders
                         where o.OrderDate >= startDate && o.OrderDate <= endDate
                         orderby o.OrderDate
                         select new
                         {
                             CustomerName = o.Customer.Name,
                             TotalRevenue = o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)
                         };


            var sql = orders.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var result = await orders.ToListAsync();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> OrderItemGroupByProduct()
        {
            var query = _context.OrderItems
                .GroupBy(oi => oi.Product)
                .Select(g => new
                {
                    productName = g.Select(oi => oi.Product.Name),
                    orderItemId = g.Select(oi => oi.OrderItemId),
                    quantity = g.Select(oi => oi.Quantity)
                });


            var sql = query.ToQueryString();
            Console.WriteLine($"\n\n\nCompiled SQL: {sql}\n\n\n");
            var result = await query.ToListAsync();
            return Ok(result);
        }
    }
}

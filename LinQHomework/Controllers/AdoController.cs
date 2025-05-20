using LinQHomework.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LinQHomework.Data;

namespace LinQHomework.Controllers
{
    public class AdoController : Controller
    {
        private readonly AppDbContext _context;


        private readonly IConfiguration _configuration; // for ADO.NET raw operation (to compare performance with EF)
        private readonly string _connectionString;

        public AdoController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MSSQLServer"); // specify to match your appsettings.json
        }

        // Worked but: wrong column mapped (since both Product & Customer have "Name" field)
        // The issue is that your SQL query is selecting
        // all columns (o.*, c.*, oi.*, p.*) without proper
        // column name disambiguation, causing name collisions
        // between Customer.Name and Product.Name.
        [HttpGet("[action]")]
        public async Task<IActionResult> RetriveJoinedOrdersADOWrongField()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("[ADO.NET] Starting RetriveJoinedOrders");

            string sql = @"
            SELECT 
                o.*,
                c.*,
                oi.*,
                p.*
            FROM Orders o
            INNER JOIN Customers c ON o.CustomerId = c.CustomerId
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            ORDER BY o.OrderId";

            Console.WriteLine($"\n\n\nRaw SQL: {sql}\n\n\n");

            var result = new List<dynamic>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine($"[ADO.NET] Connection opened in {stopwatch.ElapsedMilliseconds}ms");

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    stopwatch.Restart();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine($"[ADO.NET] Query executed in {stopwatch.ElapsedMilliseconds}ms");

                        while (await reader.ReadAsync())
                        {
                            var customer = new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                // Add all other Customer properties
                            };

                            var order = new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                Customer = customer,
                                // Add all other Order properties
                            };

                            var orderItem = new OrderItem
                            {
                                OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                // Add all other OrderItem properties
                            };

                            var product = new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                // Add all other Product properties
                            };

                            result.Add(new
                            {
                                Order = order,
                                Customer = customer,
                                OrderItem = orderItem,
                                Product = product
                            });
                        }
                    }
                }

                await connection.CloseAsync();
            }

            Console.WriteLine($"[ADO.NET] Total execution time: {stopwatch.ElapsedMilliseconds}ms");
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RetriveJoinedOrdersADOCorrected()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("[ADO.NET] Starting RetriveJoinedOrders");

            // We're dealing with just two naming conflicts
            // (Product.Name and Customer.Name),
            // we only need to alias these 2 specific columns while
            // keeping the rest unchanged.
            string sql = @"
            SELECT 
                o.OrderId, o.OrderDate, o.CustomerId,
                c.CustomerId, 
                c.Name AS Customer_Name, 
                c.Email,
                oi.OrderItemId, oi.OrderId AS OrderItem_OrderId, 
                oi.ProductId AS OrderItem_ProductId, oi.Quantity,
                p.ProductId, 
                p.Name AS Product_Name, 
                p.Price
            FROM Orders o
            INNER JOIN Customers c ON o.CustomerId = c.CustomerId
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            ORDER BY o.OrderId";

            Console.WriteLine($"\n\n\nRaw SQL: {sql}\n\n\n");

            var result = new List<dynamic>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine($"[ADO.NET] Connection opened in {stopwatch.ElapsedMilliseconds}ms");

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    stopwatch.Restart();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine($"[ADO.NET] Query executed in {stopwatch.ElapsedMilliseconds}ms");

                        while (await reader.ReadAsync())
                        {
                            var customer = new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Name = reader.GetString(reader.GetOrdinal("Customer_Name")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                // Add all other Customer properties (if needed or leave it null)
                            };

                            var order = new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                Customer = customer,
                                // Add all other Order properties (if needed or leave it null)
                            };

                            var orderItem = new OrderItem
                            {
                                OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                // Add all other OrderItem properties (if needed or leave it null)
                            };

                            var product = new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Product_Name")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                // Add all other Product properties (if needed or leave it null)
                            };

                            result.Add(new
                            {
                                Order = order,
                                Customer = customer,
                                OrderItem = orderItem,
                                Product = product
                            });
                        }
                    }
                }

                await connection.CloseAsync();
            }

            Console.WriteLine($"[ADO.NET] Total execution time: {stopwatch.ElapsedMilliseconds}ms");
            return Ok(result);
        }

        // from the EF version LinQController->RetrieveOrdersIncludeAllProps(),
        // the printed Compiled SQL has dupplications and not usable
        // the query that outputted twice by the EF info log is usable !!
        [HttpGet("[action]")]
        public async Task<IActionResult> RetrieveOrdersIncludeAllPropsADO()
        {
            string query = "SELECT [o].[OrderId], [o].[OrderDate], [c].[CustomerId], [c].[Email], [c].[Name], [t].[OrderItemId], [t].[OrderId], [t].[Quantity], [t].[ProductId], [t].[Name], [t].[Price], [t].[ProductId0], (" +
                "\r\n          SELECT COALESCE(SUM(CAST([o1].[Quantity] AS decimal(18,2)) * [p0].[Price]), 0.0)\r\n          FROM [OrderItems] AS [o1]" +
                "\r\n          INNER JOIN [Products] AS [p0] ON [o1].[ProductId] = [p0].[ProductId]" +
                "\r\n          WHERE [o].[OrderId] = [o1].[OrderId])\r\n      FROM [Orders] AS [o]\r\n      INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[CustomerId]" +
                "\r\n      LEFT JOIN (\r\n          SELECT [o0].[OrderItemId], [o0].[OrderId], [o0].[Quantity], [p].[ProductId], [p].[Name], [p].[Price], [o0].[ProductId] AS [ProductId0]" +
                "\r\n          FROM [OrderItems] AS [o0]\r\n          INNER JOIN [Products] AS [p] ON [o0].[ProductId] = [p].[ProductId]\r\n      ) AS [t] ON [o].[OrderId] = [t].[OrderId]" +
                "\r\n      ORDER BY [o].[OrderId], [c].[CustomerId], [t].[OrderId], [t].[ProductId0]";
            //string connectionString = _configuration.GetConnectionString("IdentityConnection");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                List<object> orders = new List<object>();
                object order = null;

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    //command.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string OderId = reader[0].ToString();
                            string OrderDate = Convert.ToDateTime(reader[1]).ToString("dd/MM/yyyy");
                            order = new
                            {
                                OderId = OderId,
                                OderDate = OrderDate,
                            };
                            orders.Add(order);
                        }
                    }
                }
                await connection.CloseAsync();
                return Ok(orders);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RetrieveOrdersIncludeAllPropsADOChatGptFix()
        {
            string query = "SELECT " +
                           "[o].[OrderId], [o].[OrderDate], " +
                           "[c].[CustomerId], [c].[Name], [c].[Email], " +
                           "[oi].[OrderItemId], [oi].[ProductId], [oi].[Quantity], " +
                           "[p].[Name] AS ProductName, [p].[Price] " +
                           "FROM [Orders] AS [o] " +
                           "INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[CustomerId] " +
                           "INNER JOIN [OrderItems] AS [oi] ON [o].[OrderId] = [oi].[OrderId] " +
                           "INNER JOIN [Products] AS [p] ON [oi].[ProductId] = [p].[ProductId]";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                List<object> orders = new List<object>();
                Dictionary<int, object> orderMap = new Dictionary<int, object>();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int orderId = Convert.ToInt32(reader["OrderId"]);
                            if (!orderMap.ContainsKey(orderId))
                            {
                                orderMap[orderId] = new
                                {
                                    OrderId = orderId,
                                    OrderDate = Convert.ToDateTime(reader["OrderDate"]).ToString("yyyy-MM-dd"),
                                    Customer = new
                                    {
                                        CustomerId = Convert.ToInt32(reader["CustomerId"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString()
                                    },
                                    OrderItems = new List<object>(),
                                    Total = 0.0
                                };
                            }

                            dynamic order = orderMap[orderId];
                            order.OrderItems.Add(new
                            {
                                OrderItemId = Convert.ToInt32(reader["OrderItemId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Product = new
                                {
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    Name = reader["ProductName"].ToString(),
                                    Price = Convert.ToDouble(reader["Price"])
                                }
                            });

                            //order.Total += Convert.ToDecimal(reader["Quantity"]) * Convert.ToDecimal(reader["Price"]);
                            //order.Total = order.Total + Convert.ToDecimal(reader["Quantity"]) * Convert.ToDecimal(reader["Price"]);
                            //order.Total = order.Total + Convert.ToDouble(reader["Quantity"]) * Convert.ToDouble(reader["Price"]);
                        }
                    }
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int orderId = Convert.ToInt32(reader["OrderId"]);
                            if (!orderMap.ContainsKey(orderId))
                            {
                                orderMap[orderId] = new
                                {
                                    OrderId = orderId,
                                    OrderDate = Convert.ToDateTime(reader["OrderDate"]).ToString("yyyy-MM-dd"),
                                    Customer = new
                                    {
                                        CustomerId = Convert.ToInt32(reader["CustomerId"]),
                                        Name = reader["Name"].ToString(),
                                        Email = reader["Email"].ToString()
                                    },
                                    OrderItems = new List<object>(),
                                    Total = 0.0m
                                };
                            }

                            dynamic order = orderMap[orderId];
                            order.OrderItems.Add(new
                            {
                                OrderItemId = Convert.ToInt32(reader["OrderItemId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Product = new
                                {
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    Name = reader["ProductName"].ToString(),
                                    Price = reader["Price"]
                                }
                            });

                            // still got errors with decimal, but decimal is used for stuffs that involve money.
                            // order.Total += Convert.ToDouble(Convert.ToDecimal(reader["Quantity"]) * Convert.ToDecimal(reader["Price"]));
                        }
                    }
                }

                foreach (var order in orderMap.Values)
                {
                    orders.Add(order);
                }

                await connection.CloseAsync();
                return Ok(orders);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration;
using NuGet.Packaging.Signing;
using PhuKienShop.Data;
using PhuKienShop.Models;
using PhuKienShop.Services;
using static NuGet.Packaging.PackagingConstants;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : Controller
    {
        private readonly PkShopContext _context;
    
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;


        public OrdersController(PkShopContext context, IOrderDetailService orderDetailService, IProductService productService)
        {
            _context = context;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

   

  

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate) // Sắp xếp theo thứ tự giảm dần của OrderDate (từ mới nhất đến cũ nhất)
                .ToListAsync();

            var orderViewModels = orders
                  .Select(o => new OrderViewModel
                  {
                      Order = o
                  })
                  .ToList();

                    return View(orderViewModels);

         

        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CreatedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,OrderDate,TotalAmount,Status,CreatedAt")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        // lay trang thai order
        public async Task<string> GetOrderStatusById(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                return order.Status;
            }

            return null;
        }
        
        public async Task<Order> GetOrderById(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null) return order;
            else return null;
        }

        public async Task<int> UpdateStatus(int? id, string action)
        {
            if (id == null)
            {
                return 0;
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return 0;
            }

            action = action.ToUpper() + "ED";
            order.Status = action;
            DateTime time = DateTime.Now;
            order.CreatedAt = time;

            _context.Orders.Update(order);
            int rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected;
        }

        public async Task<Dictionary<Order,string>> selectFullOrderByID(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            List<int> oids = new List<int>();
            order = order ?? new Order(0);
            oids.Add(order.OrderId);
            

            var orderDetails = await _orderDetailService.SelectByOrdersAsync(oids);
            List<int> pids = new List<int>();
            foreach (var orderDetail in orderDetails)
            {
                if (orderDetail.ProductId.HasValue)
                {
                    if (!pids.Contains(orderDetail.ProductId.Value))
                        pids.Add(orderDetail.ProductId.Value);
                }
            }

            var products = await _productService.SelectByIDsAsync(pids);
            Dictionary<int, string> productInfo = new Dictionary<int, string>();
            foreach (var p in products)
            {
                productInfo.Add(p.ProductId, p.ProductName);
            }

            Dictionary<Order, string> orders = new Dictionary<Order, string>();
            orders.Add(order, "");
            

            string detail = "";
            string name = "";
            Order temp;
            foreach (var de in orderDetails)
            {
                int pid = de.ProductId.GetValueOrDefault(); // Sẽ trả về giá trị 0 nếu ProductId là null
                name = productInfo[pid];
                detail = de.Quantity + " x " + name;

                int oid = de.OrderId.GetValueOrDefault();
                temp = new Order(oid);
                if (orders.ContainsKey(temp))
                {
                    string newDatail = orders[temp] + " <br> " + detail;
                    orders[temp] = newDatail;

                }
                else
                {
                    orders.Add(temp, detail);
                }
            }
            return orders;

        }

        [HttpPost]
        public async Task<IActionResult> updateOrder(string id, string action)
        {
           
            Console.WriteLine($"id: {id}");
            Console.WriteLine($"action: {action}");
            Console.WriteLine("goi update");

            int orderId = int.Parse(id);
            var order = await _context.Orders.FindAsync(orderId);

            if (action == null)
            {
                Console.WriteLine("cap nhat that bai do sai action");
                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Hành động không hợp lệ.");
                return PartialView("_OrderUpdateFailed", failed);
            }
            if (order != null)
            {
               action = action.ToUpper();
               switch(action)
                {
                    case "CONFIRM":
                        {
                            string currentStatus = await GetOrderStatusById(orderId);
                            currentStatus = currentStatus ?? "";
                            currentStatus = currentStatus.ToUpper();    
                            //kiem tra cac trang thai co the thuc hien action
                            if ("WAITING".Equals(currentStatus))
                            {
                                int re = await UpdateStatus(orderId, action);

                                if(re>0) { //cap nhat thanh cong
                                var orderDetails = await selectFullOrderByID(orderId);
                                var firstOrderDetail = orderDetails.FirstOrDefault();
                                OrderViewModel success = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
                                    return PartialView("_OrderRowUpdate", success);
                                } else //cap nhat that bai 
                                {
                                    Console.WriteLine("cap nhat that bai ko ro lyu do");
                                    OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Có lỗi truy vấn dữ liệu.");
                                    return PartialView("_OrderUpdateFailed", failed);
                                }
                            }
                            else //action khong phu hop
                            {
                                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Trạng thái đơn hàng {Order.tranlateOrderStatus(currentStatus)}");
                                return PartialView("_OrderUpdateFailed", failed);
                            }
                            break;
                        }
                    case "PACKAGE":
                        {
                            List<string> statusCanUpdate = new List<string>();
                            statusCanUpdate.Add("WAITING");
                            statusCanUpdate.Add("CONFIRMED");
                            string currentStatus = await GetOrderStatusById(orderId);
                            currentStatus = currentStatus ?? "";
                            currentStatus = currentStatus.ToUpper();
                            //kiem tra cac trang thai co the thuc hien action
                            if (statusCanUpdate.Contains(currentStatus))
                            {
                                int re = await UpdateStatus(orderId, action);

                                if (re > 0)
                                {
                                    var orderDetails = await selectFullOrderByID(orderId);
                                    var firstOrderDetail = orderDetails.FirstOrDefault();
                                    OrderViewModel success = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
                                    // Trả về partial view dưới dạng HTML
                                    Console.WriteLine("cap nhat thanh cong");
                                    return PartialView("_OrderRowUpdate", success);
                                }
                                else
                                {
                                    OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Có lỗi truy vấn dữ liệu.");
                                    return PartialView("_OrderUpdateFailed", failed);
                                }
                            }
                            else
                            {
                                Console.WriteLine("cap nhat that bai do sai action");
                                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Trạng thái đơn hàng {Order.tranlateOrderStatus(currentStatus)}");
                                // Trả về partial view dưới dạng HTML
                                return PartialView("_OrderUpdateFailed", failed);
                            }
                            break;
                        }
                    case "DELIVERY":
                        {
                            List<string> statusCanUpdate = new List<string>();
                            statusCanUpdate.Add("WAITING");
                            statusCanUpdate.Add("CONFIRMED");
                            statusCanUpdate.Add("PACKAGEED");
                            string currentStatus = await GetOrderStatusById(orderId);
                            currentStatus = currentStatus ?? "";
                            currentStatus = currentStatus.ToUpper();
                            //kiem tra cac trang thai co the thuc hien action
                            if (statusCanUpdate.Contains(currentStatus))
                            {
                                int re = await UpdateStatus(orderId, action);

                                if (re > 0)
                                {
                                    var orderDetails = await selectFullOrderByID(orderId);
                                    var firstOrderDetail = orderDetails.FirstOrDefault();
                                    OrderViewModel success = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
                                    // Trả về partial view dưới dạng HTML
                                    return PartialView("_OrderRowUpdate", success);
                                }
                                else
                                {
                                    OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Có lỗi truy vấn dữ liệu.");
                                    return PartialView("_OrderUpdateFailed", failed);

                                }
                            }
                            else
                            {
                                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Trạng thái đơn hàng {Order.tranlateOrderStatus(currentStatus)}");
                                // Trả về partial view dưới dạng HTML
                                return PartialView("_OrderUpdateFailed", failed);
                            }
                            break;
                        }
                    case "COMPLETE":
                        {
                            List<string> statusCanUpdate = new List<string>();
                            statusCanUpdate.Add("WAITING");
                            statusCanUpdate.Add("CONFIRMED");
                            statusCanUpdate.Add("PACKAGEED");
                            statusCanUpdate.Add("DELIVERYED");
                            string currentStatus = await GetOrderStatusById(orderId);
                            currentStatus = currentStatus ?? "";
                            currentStatus = currentStatus.ToUpper();
                            //kiem tra cac trang thai co the thuc hien action
                            if (statusCanUpdate.Contains(currentStatus))
                            {
                                int re = await UpdateStatus(orderId, action);

                                if (re > 0)
                                {
                                    var orderDetails = await selectFullOrderByID(orderId);
                                    var firstOrderDetail = orderDetails.FirstOrDefault();
                                    OrderViewModel success = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
                                    // Trả về partial view dưới dạng HTML
                                    return PartialView("_OrderRowUpdate", success);
                                }
                                else
                                {
                                    Console.WriteLine("cap nhat that bai ko ro lyu do");
                                    OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Có lỗi truy vấn dữ liệu.");
                                    return PartialView("_OrderUpdateFailed", failed);

                                }
                            }
                            else
                            {
                                Console.WriteLine("cap nhat that bai do sai action");
                                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Trạng thái đơn hàng {Order.tranlateOrderStatus(currentStatus)}");
                                // Trả về partial view dưới dạng HTML
                                return PartialView("_OrderUpdateFailed", failed);
                            }
                            break;
                        }
                    case "CANCEL":
                        {
                            List<string> statusCanUpdate = new List<string>();
                            statusCanUpdate.Add("WAITING");
                            statusCanUpdate.Add("CONFIRMED");
                            statusCanUpdate.Add("PACKAGEED");
                            string currentStatus = await GetOrderStatusById(orderId);
                            currentStatus = currentStatus ?? "";
                            currentStatus = currentStatus.ToUpper();
                            //kiem tra cac trang thai co the thuc hien action
                            if (statusCanUpdate.Contains(currentStatus))
                            {
                                int re = await UpdateStatus(orderId, action);

                                if (re > 0)
                                {
                                    var orderDetails = await selectFullOrderByID(orderId);
                                    var firstOrderDetail = orderDetails.FirstOrDefault();
                                    OrderViewModel success = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
                                    // Trả về partial view dưới dạng HTML
                                    return PartialView("_OrderRowUpdate", success);
                                }
                                else
                                {
                                    OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Có lỗi truy vấn dữ liệu.");
                                    return PartialView("_OrderUpdateFailed", failed);

                                }
                            }
                            else
                            {
                                Console.WriteLine("cap nhat that bai do sai action");
                                OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Trạng thái đơn hàng {Order.tranlateOrderStatus(currentStatus)}");
                                // Trả về partial view dưới dạng HTML
                                return PartialView("_OrderUpdateFailed", failed);
                            }
                            break;
                        }
                    default:
                        {
                            OrderViewModel failed = new OrderViewModel(orderId, $"Thất bại. Hành động không hợp lệ.");
                            return PartialView("_OrderUpdateFailed", failed);
                        }
                }
            } 
            else
            {
                OrderViewModel notfound = new OrderViewModel(orderId, $"Thất bại. Không tìm thấy đơn hàng {orderId}");
                return PartialView("_OrderUpdateFailed", notfound);
            }

       
        }

        [HttpPost]
        public async Task<IActionResult> orderDetail(string idin)
        {
            int id = int.Parse(idin);
            Dictionary<Order, string> orderDetails = await selectFullOrderByID(id);
            var firstOrderDetail = orderDetails.FirstOrDefault();
            OrderViewModel re = new OrderViewModel(firstOrderDetail.Key, firstOrderDetail.Value);
            return PartialView("_OrderDetail", re);

        }

        [HttpPost]
        public async Task<IActionResult> searchOrder(string idin)
        {
            if(idin==null)
            {
                var orders = await _context.Orders
                 .OrderByDescending(o => o.OrderDate) // Sắp xếp theo thứ tự giảm dần của OrderDate (từ mới nhất đến cũ nhất)
                 .ToListAsync();

                var orderViewModels = orders
                      .Select(o => new OrderViewModel
                      {
                          Order = o
                      })
                      .ToList();

                return View(orderViewModels);

            }
            else
            {
                int id = int.Parse(idin);
                Order order = await GetOrderById(id);
                OrderViewModel re = new OrderViewModel(order);
                return PartialView("_OrderRow", re);
            }
           

        }
    }

    
}

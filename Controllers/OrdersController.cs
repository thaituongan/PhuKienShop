using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;
using PhuKienShop.Services;

namespace PhuKienShop.Controllers
{
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
            var pkShopContext = _context.Orders.Include(o => o.User);
            List<int> oids = new List<int>();
            foreach(var i in pkShopContext)
            {
                oids.Add(i.OrderId);
            }

            var orderDetails = await _orderDetailService.SelectByOrdersAsync(oids);
            List<int> pids = new List<int>();
            foreach (var orderDetail in orderDetails)
            {
                if (orderDetail.ProductId.HasValue)
                {
                    if(!pids.Contains(orderDetail.ProductId.Value))
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
            foreach(var o in pkShopContext)
            {
                orders.Add(o, "");
            }

            string detail = "";
            string name = "";
            Order temp;
            Console.WriteLine("bat dau lay detail");
            foreach (var de in orderDetails)
            {
                int pid = de.ProductId.GetValueOrDefault(); // Sẽ trả về giá trị 0 nếu ProductId là null
                name = productInfo[pid];
                detail = de.Quantity + " x " + name;
                Console.WriteLine(detail);

                int oid = de.OrderId.GetValueOrDefault();
                Console.WriteLine($"oid: {oid}");
                temp = new Order(oid);
                Console.WriteLine($"order temp id: {temp.OrderId}");
                if (orders.ContainsKey(temp))
                {
                    Console.WriteLine($"if true");
                    string newDatail = orders[temp] + " <br> " + detail;
                    orders[temp]= newDatail;
                    Console.WriteLine($"new detail: {detail}");

                }
                else
                {
                    Console.WriteLine($"if false");
                    orders.Add(temp, detail);
                }
            }
            Console.WriteLine($"duyet qua dictionary");
         
            foreach (KeyValuePair<Order, string> item in orders)
            {
                Console.WriteLine(item.Key.OrderId + "\t"+ item.Key.OrderId + "\t" + item.Value);
            }
            Console.WriteLine($"ket thuc duyet");

            var orderViewModels = pkShopContext
                  .Select(o => new OrderViewModel
                  {
                      Order = o,
                      Details = orders.ContainsKey(o) ? orders[o] : string.Empty
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
    }
}

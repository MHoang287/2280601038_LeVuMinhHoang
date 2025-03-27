using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2280601038_LeVuMinhHoang.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using _2280601038_LeVuMinhHoang.Extensions;

namespace _2280601038_LeVuMinhHoang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.ApplicationUser).ToListAsync();
            return View(orders);
        }

        // GET: Admin/Order/Edit/5
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
            return View(order);
        }

        // POST: Admin/Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderDate,Total,UserId")] Order order)
        {
            if (id != order.Id)
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
                    if (!OrderExists(order.Id))
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
            return View(order);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.UserId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.UserId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Checkout()
        {
            try
            {
                var cart = HttpContext.Session.GetObject<ShoppingCart>("Cart") ?? new ShoppingCart();

                // Đảm bảo Items không null
                cart.Items ??= new List<CartItem>();

                var order = new Order
                {
                    SubTotal = cart.GetSubTotal(),
                    ShippingFee = cart.GetSubTotal() >= 500000 ? 0 : 20000,
                    Tax = cart.CalculateTax(cart.GetSubTotal()),
                    TotalPrice = cart.CalculateTotal(
                        cart.GetSubTotal(),
                        cart.GetSubTotal() >= 500000 ? 0 : 20000,
                        cart.CalculateTax(cart.GetSubTotal()))
                };

                ViewBag.Cart = cart;
                return View(order);
            }
            catch (Exception ex)
            {
                // Log lỗi và chuyển hướng về trang giỏ hàng
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                var cart = HttpContext.Session.GetObject<ShoppingCart>("Cart");

                // Kiểm tra giỏ hàng hợp lệ
                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    return RedirectToAction("Index", "ShoppingCart");
                }

                if (ModelState.IsValid)
                {
                    // Điền thông tin đơn hàng
                    order.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    order.OrderDate = DateTime.Now;
                    order.SubTotal = cart.GetSubTotal();
                    order.ShippingFee = cart.GetSubTotal() >= 500000 ? 0 : 20000;
                    order.Tax = cart.CalculateTax(cart.GetSubTotal());
                    order.TotalPrice = cart.CalculateTotal(order.SubTotal, order.ShippingFee, order.Tax);
                    order.OrderDetails = new List<OrderDetail>();

                    // Thêm chi tiết đơn hàng
                    foreach (var item in cart.Items.Where(i => i != null))
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        });
                    }

                    // Lưu đơn hàng
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Xóa giỏ hàng
                    HttpContext.Session.Remove("Cart");

                    return RedirectToAction("Confirmation", new { id = order.Id });
                }

                ViewBag.Cart = cart;
                return View(order);
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo
                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message);
                return View(order);
            }
        }

        public IActionResult Confirmation(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
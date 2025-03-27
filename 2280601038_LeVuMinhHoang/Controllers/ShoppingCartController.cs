using _2280601038_LeVuMinhHoang.Extensions;
using _2280601038_LeVuMinhHoang.Models;
using _2280601038_LeVuMinhHoang.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace _2280601038_LeVuMinhHoang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository,
            ILogger<ShoppingCartController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        private string GetCartSessionKey()
        {
            if (User.Identity.IsAuthenticated)
            {
                return $"Cart_{_userManager.GetUserId(User)}";
            }
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey) ?? new ShoppingCart();
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shopping cart");
                TempData["ErrorMessage"] = "Error loading your shopping cart";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra số lượng hợp lệ
                if (quantity <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Số lượng phải lớn hơn 0"
                    });
                }

                // Lấy thông tin sản phẩm
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

                // Xử lý giỏ hàng
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey) ?? new ShoppingCart();

                // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.AddItem(new CartItem
                    {
                        ProductId = productId,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = quantity
                    });
                }

                // Lưu giỏ hàng vào session
                HttpContext.Session.SetObjectAsJson(cartKey, cart);

                // Trả về kết quả JSON
                return Json(new
                {
                    success = true,
                    cartCount = cart.Items.Sum(i => i.Quantity),
                    message = $"Đã thêm {product.Name} vào giỏ hàng"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi thêm sản phẩm {productId} vào giỏ hàng");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi thêm vào giỏ hàng"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            try
            {
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
                }

                // Tính toán số lượng mới
                int newQuantity = item.Quantity + quantity;

                // Kiểm tra số lượng tối thiểu là 1
                if (newQuantity < 1)
                {
                    newQuantity = 1;
                }

                // Kiểm tra số lượng tồn kho (nếu cần)
                // var product = await _productRepository.GetByIdAsync(productId);
                // if (newQuantity > product.StockQuantity) {
                //     return Json(new { success = false, message = "Số lượng vượt quá tồn kho" });
                // }

                item.Quantity = newQuantity;
                HttpContext.Session.SetObjectAsJson(cartKey, cart);

                return Json(new
                {
                    success = true,
                    newQuantity = newQuantity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật số lượng sản phẩm {productId}");
                return Json(new { success = false, message = "Lỗi khi cập nhật giỏ hàng" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey);
                if (cart == null)
                {
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Product not found in cart";
                    return RedirectToAction("Index");
                }

                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson(cartKey, cart);
                TempData["SuccessMessage"] = $"{item.Name} removed from cart";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product {productId} from cart");
                TempData["ErrorMessage"] = "Error removing product from cart";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            try
            {
                var cartKey = GetCartSessionKey();
                HttpContext.Session.Remove(cartKey);
                TempData["SuccessMessage"] = "Cart cleared successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["ErrorMessage"] = "Error clearing cart";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            try
            {
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey);
                if (cart == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index");
                }

                // Tính toán các giá trị
                decimal subTotal = cart.GetSubTotal();
                decimal tax = cart.CalculateTax(subTotal);
                decimal shippingFee = 20000; // Phí ship mặc định
                decimal total = cart.CalculateTotal(subTotal, shippingFee, tax);

                // Truyền dữ liệu qua ViewBag
                ViewBag.SubTotal = subTotal;
                ViewBag.Tax = tax;
                ViewBag.Total = total;

                var order = new Order
                {
                    ShippingFee = shippingFee
                };

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["ErrorMessage"] = "Error loading checkout";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey);
                if (cart == null || !cart.Items.Any())
                {
                    ModelState.AddModelError("", "Your cart is empty");
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    return View(order);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction("Index");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tính toán các khoản phí
                    decimal subTotal = cart.GetSubTotal();
                    decimal tax = cart.CalculateTax(subTotal);
                    decimal total = cart.CalculateTotal(subTotal, order.ShippingFee, tax);

                    order.UserId = user.Id;
                    order.OrderDate = DateTime.UtcNow;
                    order.SubTotal = subTotal;
                    order.Tax = tax;
                    order.TotalPrice = total;
                    order.OrderDetails = cart.Items.Select(i => new OrderDetail
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList();

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    HttpContext.Session.Remove(cartKey);
                    return RedirectToAction("OrderConfirmation", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during checkout transaction");
                    TempData["ErrorMessage"] = "Error processing your order. Please try again.";
                    return View(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                TempData["ErrorMessage"] = "Error processing your order. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found";
                    return RedirectToAction("Index");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order confirmation {id}");
                TempData["ErrorMessage"] = "Error loading order confirmation";
                return RedirectToAction("Index");
            }
        }
    }
}
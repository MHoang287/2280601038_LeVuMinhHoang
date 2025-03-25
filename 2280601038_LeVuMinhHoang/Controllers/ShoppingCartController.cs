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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Quantity must be at least 1";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                var product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return RedirectToAction("Index", "Product");
                }

                var cartKey = GetCartSessionKey();
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(cartKey) ?? new ShoppingCart();

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

                HttpContext.Session.SetObjectAsJson(cartKey, cart);
                TempData["SuccessMessage"] = $"{product.Name} added to cart";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {productId} to cart");
                TempData["ErrorMessage"] = "Error adding product to cart";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCartItem(int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return RemoveFromCart(productId);
                }

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

                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson(cartKey, cart);
                TempData["SuccessMessage"] = "Cart updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart item {productId}");
                TempData["ErrorMessage"] = "Error updating cart";
                return RedirectToAction("Index");
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

                return View(new Order());
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
                    order.UserId = user.Id;
                    order.OrderDate = DateTime.UtcNow;
                    order.TotalPrice = cart.GetTotal();
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
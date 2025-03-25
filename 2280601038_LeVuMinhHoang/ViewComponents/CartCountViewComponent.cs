using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using _2280601038_LeVuMinhHoang.Models;

namespace _2280601038_LeVuMinhHoang.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartCountViewComponent(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var count = 0;

                if (!string.IsNullOrEmpty(userId))
                {
                    count = await _context.CartItems
                        .Where(c => c.UserId == userId)
                        .SumAsync(c => c.Quantity);
                }

                return View(count);
            }
            catch (Exception ex)
            {
                // Log error here if needed
                return View(0);
            }
        }
    }
}
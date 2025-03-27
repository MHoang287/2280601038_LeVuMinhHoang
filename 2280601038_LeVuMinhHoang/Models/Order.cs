using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace _2280601038_LeVuMinhHoang.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng tiền hàng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Phí vận chuyển")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 20000; // Mặc định 20,000đ

        [Display(Name = "Thuế (10%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Display(Name = "Tổng thanh toán")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _2280601038_LeVuMinhHoang.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal Total { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
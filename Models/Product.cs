using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models
{
    public class Product
    {
        public Product()
        {
            TempCount = 1;
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(1,int.MaxValue)]
        public double Price { get; set; }
        public string Image { get; set; }
        [Display(Name="Category Type")]

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        [Display(Name = "Application Type")]
        public int ApplicationTypeId { get; set; }
        [ForeignKey("ApplicationTypeId")]
        public virtual ApplicationType ApplicationType { get; set; }

        [NotMapped] //чтобы не добавлять миграции
        [Range(1, 10000)] //проверка на ограничение диапазона
        public int TempCount { get; set; }
    }
}

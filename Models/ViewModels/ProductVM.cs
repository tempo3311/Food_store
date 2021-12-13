using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; } //свойство типа Product
        public IEnumerable<SelectListItem> CategorySelectList { get; set; } //свойство для списка меню
        public IEnumerable<SelectListItem> ApplicationTypeSelectList { get; set; }
    }
}

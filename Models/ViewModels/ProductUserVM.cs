using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            ProductList = new List<Product>(); //если не создать этот объект внутри контроллера, это не приведёт к ошибке, потмоу что этот объект уже будет инициализирован во ViewModel для списка товаров
        }

        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<Product> ProductList { get; set; }

    }
}

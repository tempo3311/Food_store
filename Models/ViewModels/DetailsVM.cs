using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Product = new Product(); //создание нового экземпляра товара (инициализация нового объекта Product (чтобы не получить ошибку в контроллере))
        }

        public Product Product { get; set; }
        public bool ExistsInCart { get; set; } //логический флаг (По умолчанию тут не задано значение)
    }
}

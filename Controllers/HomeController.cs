using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Food_store.Models;
using Food_store.Data;
using Food_store.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Food_store.Utility;

namespace Food_store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                //заполняем свойства для нашей ViewModel (homeVM):
                Products = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType), //добавляем Products из БД. И в Products так же показываем Category и ApplicationType для страницы Home (с помощью egger loading)
                Categories = _db.Category //извлекаем список категорий
            };

            return View(homeVM); //передаём homeVM в наше View
        }

        public IActionResult Details(int id) //получение детайлей о товаре
        {
            //извлекаем сессию (для изменения кнопки Add to cart на Remove):
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>(); //создаём список для корзины покупок, потому что нужно получить сессию и посмотреть, есть ли что-нибудь в этой сессии или нет  
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null //проверяем, существует ли сессия
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart); //получаем сессию
            }


            //получаем продукт из БД на  основе id, который передаём внутрь раздела Details (последняя строчка в _IndividualProductCard)
            DetailsVM DetailsVM = new DetailsVM()
            {
                Product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType)
                .Where(u => u.Id == id).FirstOrDefault(), //если возвращается более 1 записи, то присвоим Product только первую запись
                ExistsInCart = false
            };

            foreach (var item in shoppingCartList) //проверяем, находится ли выбранный товар в списке
            {
                if (item.ProductId == id) //если товар включен в корзину покупок 
                {
                    DetailsVM.ExistsInCart = true; //меняем логический флаг на истину
                }
            }



            return View(DetailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id, DetailsVM detailsVM) //detailsVM передаём явно, как параметр, потому что в этом контроллере нет ссылки на эту модель представления
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>(); //создаём список для корзины покупок, потому что нужно получить сессию и посмотреть, есть ли что-нибудь в этой сессии или нет  
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null //проверяем, существует ли сессия
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart); //получаем сессию
            }
            shoppingCartList.Add(new ShoppingCart { ProductId = id, Count = detailsVM.Product.TempCount }); //добавляем новый товар в корзину (если спиоск ещё пуст,добавляется первый элемент. а если в списке уже были элементы, то извлекаем его в этой ветке и далее добавляем новый. При этом нужно установить сессию  ↓↓↓). А так же получаем колитчество товара из detailsVM
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList); //устанавливаем сессию
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            //получаем объект shoppingCartList
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id); //находим товар внутри корзины покупок (соответствуюзий этому Id)
            if (itemToRemove != null) //проверяем, определён ли этот элемент
            {
                shoppingCartList.Remove(itemToRemove); //удаляем товар
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList); //снова устанавливаем корзину с новым списком, который уже не содержит ProductId, который был выбран
            return RedirectToAction(nameof(Index));//передаём исполнение кода в метод Index
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
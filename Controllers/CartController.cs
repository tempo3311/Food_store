using Food_store.Data;
using Food_store.Models;
using Food_store.Models.ViewModels;
using Food_store.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Food_store.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {   
            //извлекаем список товаров из корзины
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null //проверка на существование сессии (получаем к ней доступ)
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart); //извлекаем объекты из сессии для переменной shoppingCartList
            }

            //получаем каждый товар из корзины 
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList(); //получили все товары из корзины
            IEnumerable<Product> prodListTemp = _db.Product.Where(u => prodInCart.Contains(u.Id)); //это позволяет включить встроенный доступ в базу SQL, и так можно получать все объекты Product, где id сравнивается со всеми id из списка prodInCart
            IList<Product> prodList = new List<Product>(); //ЭТУ И ВЕРХНЮЮ СТРОЧКУ ПРОВЕРИТЬ НА КОММЕНТЫ

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempCount = cartObj.Count;
                prodList.Add(prodTemp);
            }


            return View(prodList);//передаём prodList в представление
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>(); //создаём новый список объектов 
            foreach (Product prod in ProdList) //перебираем все объекты Product
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Count = prod.TempCount }); //для каждого Prod из ProdList добавляяем новый объект shoppingCart
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList); //задаём эти значения для текущей сессии. Получив актуальные значения путём перебора для всех объектов в цикле foreach в классе HttpContext вызываем метод Session.Set, где для ключа WC.SessionCart устанавливаем значение shoppingCartList

            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            //получаем id пользователя, прошедшего авторизацию
            var claimsIdentity = (ClaimsIdentity)User.Identity; //получаем доступ к User.Identity
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); //объект claim будет определён, если пользователь вошел в систему,  а если не входил, не будет определён
            //var userId = User.FindFirstValue(ClaimTypes.Name);


            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null //проверка на существование сессии (получаем к ней доступ)
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart); //загружаем список из сессии
            }

            //извлекаем список товаров в корзине 
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Where(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value), //используя объект claim получаем доступ к значению идентификатора вошедшего в систему пользователя
                ProductList = prodList
            };

            //foreach (var cartObj in shoppingCartList)
            //{
            //    Product prodTemp = _db.Product.FirstOrDefault(u => u.Id == cartObj.ProductId);
            //    prodTemp.TempCount = cartObj.Count;
            //    ProductUserVM.ProductList.Append(prodTemp);
            //}




            return View(ProductUserVM);
        }



        public IActionResult Remove(int id) 
        {
            //извлекаем список товаров из корзины
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null //проверка на существование сессии (получаем к ней доступ)
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart); //извлекаем объекты из сессии для переменной shoppingCartList
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id)); //передаём методу Remove ссылку на объект из корзины, который нужно удалить (этот объект получаем из списка shoppingCartList, используя FirstOrDefault со ссылкой на этот объект на основе идентификатора )
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList); //ещё раз устанавливаем значение для сессии из WC.SessionCart со списком shoppingCartList
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>(); //создаём новый список объектов 
            foreach (Product prod in ProdList) //перебираем все объекты Product
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, Count = prod.TempCount }); //для каждого Prod из ProdList добавляяем новый объект shoppingCart
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList); //задаём эти значения для текущей сессии. Получив актуальные значения путём перебора для всех объектов в цикле foreach в классе HttpContext вызываем метод Session.Set, где для ключа WC.SessionCart устанавливаем значение shoppingCartList
            return RedirectToAction(nameof(Index));
        }
    }
}

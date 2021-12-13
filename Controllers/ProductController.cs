using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Food_store.Data;
using Food_store.Models;
using Food_store.Models.ViewModels;

namespace Food_store.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db; //Приватное только для чтения свойство
        private readonly IWebHostEnvironment _webHostEnvironment; //получение доступа к папке images/product
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment) //Получение ссылки для этого свойства с помощью конструктора
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType);

            //foreach(var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId); //obj.Category - реззултат извлечения из БД с помощью .Category.FirstOrDefault
                                                                                         //что делает код?
                                                                                         // Из всех имеющихся сущностей Product будет извлечена и присвоена модель
                                                                                         // Category на основе этого условия: _db.Category.FirstOrDefault(u=>u.id==obj.CategoryID)
                                                                                         // в FirstOrDefault согласно критерию (u=>u.id==obj.CategoryID) может быть извлечено хоть 
                                                                                         // 10 записей, но использована только первая из них и этот результат будет  присвое объекту Категория (obj.Category)
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
            //};

            return View(objList);
        }


        // Метод GET для Upsert
        public IActionResult Upsert(int? id)
        {

            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem //создание раскрывающегося списка, извлекаем из БД, 
            //                                                                                           //i является новым...далее проецируем категорию на SelectListItem
            //                                                                                           //(КОНВЕРТИРУЕМ КАТЕГОРИИ В ЭЛЕМЕНТЫ СПИСКА ДЛЯ ВЫБОРА)
            //{
            //Text = i.Name,
            //Value = i.id.ToString()
            //}); //получили объект из БД

            // ViewBag.CategoryDropDown = CategoryDropDown;  //передаём данные из контроллера в представление

            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();

            ProductVM productVM = new ProductVM() //создаём экземпляр класса ProductVM
                                                  //создание раскрывающегося списка, извлекаем из БД, 
                                                  //i является новым...далее проецируем категорию на SelectListItem
                                                  //(КОНВЕРТИРУЕМ КАТЕГОРИИ В ЭЛЕМЕНТЫ СПИСКА ДЛЯ ВЫБОРА)
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.id.ToString()
                }),
                ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.id.ToString()
                })
            };

            if (id == null) //если не существует, запрос на создание новой сущности
            {
                //для создания
                return View(productVM); //передаём ссылку на новый объект Product
            }
            else //когда id имеет какое-то определённое значение, нужно получить объект из БД
            {
                productVM.Product = _db.Product.Find(id); //извлечение продукта из БД
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }


        // Метод POST для Upsert
        [HttpPost] //Явно определяем, что это action метод типа Post
        [ValidateAntiForgeryToken] //Встроенный механизм для форм ввода, в котором добавляется токен защиты от взлома
                                   //и в post происходит проверка, что этот токен всё ещё действителен и безопасность данных сохранена
        public IActionResult Upsert(ProductVM productVM) //Передаём ссылку на текущий объект типа Category, который нужно добавить в БД
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files; //сохраняем новое загруженное изображение а переменную files, извлекая его, используя  HttpContext.Request.Form.Files
                string webRootPath = _webHostEnvironment.WebRootPath; //получаем путь к нашей папке wwwroot
                //когда известен путь, надо проверить, имеется  ли у нас изображение

                if (productVM.Product.Id == 0)
                {
                    //СОЗДАНИЕ
                    string upload = webRootPath + WC.ImagePath; //получаем путь в папку, в которой, как мы уже знаем будут хранитьсяя файлы с картинками
                    string fileName = Guid.NewGuid().ToString(); //получаем имя файла
                    string extension = Path.GetExtension(files[0].FileName); //получаем расширение файла

                    //определив значения всех этих переменных, скопируем файл в новое место, которое определяется значением upload:
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension; //обновляем ссылку на image внутри сущности Product, указав новый путь для доступа 

                    _db.Product.Add(productVM.Product); //добавление нового товара
                }
                else
                {
                    //ОБНОВЛЕНИЕ
                    var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == productVM.Product.Id); //извлекаем объект из БД (получаем актуальную сущность Product из БД на основе id продукта)
                                                                                                                  //(AsNoTracking-отключение отслеживания сущности через Entity Framework Core) 
                                                                                                                  //после извлечения из БД, нам доступны все его свойства

                    if (files.Count > 0) //если новый файл уже был получен для существующего продукта 
                    {
                        string upload = webRootPath + WC.ImagePath; //получаем путь в папку, в которой, как мы уже знаем будут хранитьсяя файлы с картинками
                        string fileName = Guid.NewGuid().ToString(); //получаем имя файла
                        string extension = Path.GetExtension(files[0].FileName); //получаем расширение файла

                        //нужно удалить старый фал перед обновлением
                        var oldFile = Path.Combine(upload, objFromDb.Image); //получаем ссылку на старое фото

                        if (System.IO.File.Exists(oldFile)) //проверяем старый путь и старый файл 
                        {
                            System.IO.File.Delete(oldFile); //если он существует, удаляем его
                        }

                        //определив значения всех этих переменных, скопируем файл в новое место, которое определяется значением upload:
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        //после того, как переместили новое изображение, сохраним ссылку на него
                        productVM.Product.Image = fileName + extension;
                    }
                    else //файл фото для загрузки не менялся, но были обновлены другие свойства товара
                    {
                        productVM.Product.Image = objFromDb.Image; //сохраняем значение свойства Image прежним, если оно не было модифицировано в переменных fileName и extension 
                    }
                    _db.Product.Update(productVM.Product); //метод обновления БД
                }

                _db.SaveChanges();
                return RedirectToAction("Index"); //Перенаправление исполнения кода в другой action метод (в Index)
            }
            productVM.CategorySelectList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.id.ToString()
            });
            productVM.ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.id.ToString()
            });
            return View(productVM); //передаём детали productVM, если состояние модели не валидно

        }



        // Метод GET для Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType).FirstOrDefault(u => u.Id == id); // "Жадная загрузка" - способ сообщить Entity Framework Core, что когда мы загружаем Product, нужно модифицировать операцию join в БД и так же загрузить соответствующую Category, если эта запись будет найдена в БД

            //product.Category = _db.Category.Find(product.CategoryId);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Метод POST для Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath; //получаем путь в папку, в которой, как мы уже знаем будут хранитьсяя файлы с картинками

            //нужно удалить старый файл перед обновлением
            var oldFile = Path.Combine(upload, obj.Image);//получаем ссылку на старое фото

            if (System.IO.File.Exists(oldFile)) //проверяем старый путь и старый файл
            {
                System.IO.File.Delete(oldFile); //если он существует, удаляем его
            }


            _db.Product.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");


        }

    }
}
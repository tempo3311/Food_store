using Food_store.Data;
using Food_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db; //Приватное только для чтения свойство

        public CategoryController(ApplicationDbContext db) //Получение ссылки для этого свойства с помощью конструктора
        {
            _db = db;
        }
        
        public IActionResult Index()
        {
            IEnumerable<Category> objList = _db.Category;
            return View(objList);
        }
        // Метод GET для Create
        public IActionResult Create()
        {
            return View();
        }

        // Метод POST для Create
        [HttpPost] //Явно определяем, что это action метод типа Post
        [ValidateAntiForgeryToken] //Встроенный механизм для форм ввода, в котором добавляется токен защиты от взлома
                                   //и в post происходит проверка, что этот токен всё ещё действителен и безопасность данных сохранена
        public IActionResult Create(Category obj) //Передаём ссылку на текущий объект типа Category, который нужно добавить в БД
        {
            if (ModelState.IsValid)
            {
                _db.Category.Add(obj); //Добавление в БД
                _db.SaveChanges(); //Метод сохранения изменений
                return RedirectToAction("Index"); //Перенаправление исполнения кода в другой action метод (в Index)
            }
            return View(obj);
        }

        // Метод GET для Edit
        public IActionResult Edit(int? id)
        {
            if(id==null || id==0)
            {
                return NotFound();
            }
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        // Метод POST для Edit
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public IActionResult Edit(Category obj) 
        {
            if (ModelState.IsValid)
            {
                _db.Category.Update(obj); 
                _db.SaveChanges(); 
                return RedirectToAction("Index"); 
            }
            return View(obj);
        }

        // Метод GET для Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        // Метод POST для Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Category.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
                _db.Category.Remove(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
        }
    }
}

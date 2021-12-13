using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models
{
    public class ApplicationType //Создание таблицы 
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}

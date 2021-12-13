using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food_store.Models
{
    public class ApplicationUser : IdentityUser //(при добавлении новых свойств в этот класс, будет настроена таблица по умолчанию AspNetUUsers)
    {
        public string FullName { get; set; }
    }
}

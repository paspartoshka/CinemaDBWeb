using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.ComponentModel.DataAnnotations;

namespace CinemaDBWeb.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        [Required(ErrorMessage = "Имя обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Фамилия обязательна")]
        public string Surname { get; set; }
        public string Job { get; set; }

        public Director Director { get; set; }
        public Actor Actor { get; set; }
    }
}

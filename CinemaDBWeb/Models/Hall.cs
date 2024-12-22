using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CinemaDBWeb.Models
{
    public class Hall
    {
        public int HallId { get; set; }

        public int HallNumber { get; set; }
        [Required(ErrorMessage = "Тип зала обязателен")]
        public string? HallType { get; set; }
        [Required(ErrorMessage = "Количество рядов обязательно")]
        public int? RowCount { get; set; }
        [Required(ErrorMessage = "Количество сидений обязательно")]
        public int? SeatCount { get; set; }
        [Required(ErrorMessage = "Тип проектора обязателен")]
        public string? ProjType { get; set; }
        [Required(ErrorMessage = "Множитель цены обязателен")]
        public decimal? PriceMult { get; set; }

        public ICollection<Session> Sessions { get; set; }

        public Hall()
        {
            Sessions = new List<Session>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;



namespace CinemaDBWeb.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        [Required(ErrorMessage = "Дата сеанса обязательна")]
        [DataType(DataType.DateTime)]
        public DateTime? Date { get; set; }
        [Required(ErrorMessage = "Базовая цена обязательна.")]
        public decimal? BasePrice { get; set; }
        [Required(ErrorMessage = "Фильм обязателен.")]
        public int? MovieId { get; set; }
        public Movie? Movie  { get; set; }
        [Required(ErrorMessage = "Зал обязателен.")]
        public int? HallId { get; set; }
        public Hall? Hall { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Promo> Promos { get; set; }

        public Session()
        {
            Tickets = new List<Ticket>();
            Promos = new List<Promo>();
        }
    }
}

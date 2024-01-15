using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TheatherApp.Models
{
    public class NewsMessage
    {
        public int? Id { get; set; }
        [Display(Name = "Nieuwsbericht titel")]
        [Required(ErrorMessage = "Titel is vereist.")]
        public string Title { get; set; }

        [Display(Name = "Bericht")]
        [Required(ErrorMessage = "Bericht mag niet leeg zijn.")]
        public string Message { get; set; }

        [Display(Name = "Datum")]
        [Required(ErrorMessage = "Datum moet ingevuld zijn.")]
        public DateTime Date { get; set; }

    }
}

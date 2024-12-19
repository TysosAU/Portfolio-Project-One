using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class HeroLoadout
    {
        public int ID { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Commander name must only contain letters and spaces.")]
        public string Commander { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Perk must only contain letters and spaces.")]
        public string Perk { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Hero One name must only contain letters and spaces.")]
        public string HeroOne { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Hero Two name must only contain letters and spaces.")]
        public string HeroTwo { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Hero Three name must only contain letters and spaces.")]
        public string HeroThree { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Hero Four name must only contain letters and spaces.")]
        public string HeroFour { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Hero Five name must only contain letters and spaces.")]
        public string HeroFive { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Gadget One must only contain letters and spaces.")]
        public string GadgetOne { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Gadget Two must only contain letters and spaces.")]
        public string GadgetTwo { get; set; }
        [Required]
        [RegularExpression(@"[A-Za-z ]+", ErrorMessage = "Created By must only contain letters and spaces.")]
        public string CreatedBy { get; set; }
    }
}

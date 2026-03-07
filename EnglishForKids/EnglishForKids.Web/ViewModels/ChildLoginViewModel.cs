using System.ComponentModel.DataAnnotations;

namespace EnglishForKids.Web.ViewModels
{
    public class ChildLoginViewModel
    {
        [Required(ErrorMessage = "Введите email родителя")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [Display(Name = "Email родителя")]
        public string ParentEmail { get; set; }

        [Required(ErrorMessage = "Введите PIN-код")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN-код должен состоять из 4 цифр")]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "PIN-код должен содержать только цифры")]
        [Display(Name = "PIN-код ребенка")]
        public string PinCode { get; set; }
    }
}
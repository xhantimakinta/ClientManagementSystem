using System.ComponentModel.DataAnnotations;

namespace ClientManagementSystem.ViewModels
{
    public class ClientCreateViewModel
    {
        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        
        [StringLength(6)]
        public string? ClientCode { get; set; }
    }
}
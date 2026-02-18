using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClientManagementSystem.Models;

namespace ClientManagementSystem.ViewModels
{
    public class ContactEditViewModel
    {
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        // Linked clients (for display)
        public List<ClientContact>? LinkedClientContacts { get; set; }

        // Available clients that are NOT linked yet (for linking)
        public List<Client>? AvailableClients { get; set; }

        // Client selected in the "Link Client" dropdown
        public int? SelectedClientId { get; set; }
    }
}

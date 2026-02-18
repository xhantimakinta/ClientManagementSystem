using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClientManagementSystem.Models;

namespace ClientManagementSystem.ViewModels
{
    public class ContactCreateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; }

        // IDs of clients selected when creating a contact
        public List<int>? SelectedClients { get; set; }

        // List of available clients to choose from
        public List<Client>? Clients { get; set; }
    }
}
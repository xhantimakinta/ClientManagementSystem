using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientManagementSystem.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }   // Primary Key

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; }

        // ✅ Add ClientCode property
        [Required(ErrorMessage = "Client code is required")]
        [StringLength(6, ErrorMessage = "Client code must be 6 characters (e.g., ABC123)")]
        public string ClientCode { get; set; }

        // Navigation property for linked contacts
        public List<ClientContact> ClientContacts { get; set; } = new();
    }
}
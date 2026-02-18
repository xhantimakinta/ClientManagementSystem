using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientManagementSystem.Models
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }   // Clear PK name, matches DB design

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

        
        public ICollection<ClientContact> ClientContacts { get; set; } = new List<ClientContact>();
    }
}
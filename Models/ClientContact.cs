using System.ComponentModel.DataAnnotations;

namespace ClientManagementSystem.Models
{
    public class ClientContact
    {
        [Key]
        public int ClientContactId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        [Required]
        public int ContactId { get; set; }
        public Contact Contact { get; set; }
    }
}
namespace ClientManagementSystem.Models
{
    public interface IClientContact
    {
        int ClientContactId { get; set; }

        int ClientId { get; set; }
        Client Client { get; set; }

        int ContactId { get; set; }
        Contact Contact { get; set; }
    }
}

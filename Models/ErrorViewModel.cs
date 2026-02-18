using System.ComponentModel.DataAnnotations;

namespace ClientManagementSystem.Models
{
    public class ErrorViewModel
    {
        [Display(Name = "Request ID")]
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
    }
}
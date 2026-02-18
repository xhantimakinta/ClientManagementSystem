using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientManagementSystem.Models;
using ClientManagementSystem.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ClientManagementSystem.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ClientManagementSystemContext _context;

        public ClientsController(ClientManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.ClientContacts)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(clients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            // Email and ClientCode are not user-entered in the spec; they are generated.
            ModelState.Remove(nameof(Client.Email));
            ModelState.Remove(nameof(Client.ClientCode));

            if (!ModelState.IsValid)
                return View(client);

            // Generate code per spec (AAA999) and ensure uniqueness against SQL
            client.ClientCode = await GenerateClientCodeAsync(client.Name);

            // The template includes Client.Email even though it isn't required by the spec.
            // To keep the DB constraint happy while keeping the UI aligned with the spec,
            // we auto-fill a predictable placeholder if the field wasn't provided.
            if (string.IsNullOrWhiteSpace(client.Email))
            {
                client.Email = $"{client.ClientCode.ToLowerInvariant()}@example.com";
            }

            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                .ThenInclude(cc => cc.Contact)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.ClientId)
                return NotFound();

            // Email is not edited in the UI; avoid failing validation on this field.
            ModelState.Remove(nameof(Client.Email));

            if (!ModelState.IsValid)
                return View(client);

            var existingClient = await _context.Clients.FindAsync(id);

            if (existingClient == null)
                return NotFound();

            existingClient.Name = client.Name;
            // Email is not part of the assessment spec UI, so we keep the existing stored value.

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Link(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                .ThenInclude(cc => cc.Contact)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null) return NotFound();

            ViewBag.AllContacts = await _context.Contacts
                .OrderBy(c => c.Surname)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Link(int clientId, int contactId)
        {
            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            var contact = await _context.Contacts.FindAsync(contactId);

            if (client == null || contact == null) return NotFound();

            if (!client.ClientContacts.Any(cc => cc.ContactId == contactId))
            {
                client.ClientContacts.Add(new ClientContact
                {
                    ClientId = clientId,
                    ContactId = contactId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> UnlinkAjax([FromBody] UnlinkDto dto)
        {
            var clientId = dto.ClientId;
            var contactId = dto.ContactId;

            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc =>
                    cc.ClientId == clientId &&
                    cc.ContactId == contactId);

            if (link == null)
                return Json(new { success = false });

            _context.ClientContacts.Remove(link);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =============================
        // UNLINK CONTACT FROM CLIENT (URL / FORM POST)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlink(int clientId, int contactId)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId);

            if (link != null)
            {
                _context.ClientContacts.Remove(link);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = clientId });
        }

        public class UnlinkDto
        {
            public int ClientId { get; set; }
            public int ContactId { get; set; }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
                return NotFound();

            if (client.ClientContacts.Any())
            {
                TempData["Error"] = "Cannot delete client with linked contacts.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateClientCodeAsync(string clientName)
        {
            var prefix = GenerateAlphaPrefix(clientName);

            // Find the latest code for this prefix (e.g., FNB009) and increment
            var lastForPrefix = await _context.Clients
                .Where(c => c.ClientCode.StartsWith(prefix))
                .OrderByDescending(c => c.ClientCode)
                .Select(c => c.ClientCode)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (!string.IsNullOrWhiteSpace(lastForPrefix) && lastForPrefix.Length == 6)
            {
                if (int.TryParse(lastForPrefix.Substring(3, 3), out var n))
                    nextNumber = n + 1;
            }

            // Safety loop to guarantee uniqueness
            while (true)
            {
                var code = $"{prefix}{nextNumber:000}";
                var exists = await _context.Clients.AnyAsync(c => c.ClientCode == code);
                if (!exists) return code;
                nextNumber++;
            }
        }

        private static string GenerateAlphaPrefix(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                return "AAA";

            var words = clientName
                .Trim()
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Spec examples:
            // "First National Bank" -> FNB (first letters of first 3 words)
            // "Protea" -> PRO (first 3 letters)
            // "IT" -> ITA (pad with A)
            if (words.Length >= 3)
            {
                return string.Concat(words.Take(3).Select(w => w[0]));
            }

            var raw = new string(clientName
                .Trim()
                .ToUpperInvariant()
                .Where(char.IsLetter)
                .ToArray());

            if (raw.Length >= 3)
                return raw.Substring(0, 3);

            return raw.PadRight(3, 'A').Substring(0, 3);
        }
    }
}
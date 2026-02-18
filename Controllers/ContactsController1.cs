using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClientManagementSystem.Models;
using ClientManagementSystem.Data;
using ClientManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ClientManagementSystem.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ClientManagementSystemContext _context;

        public ContactsController(ClientManagementSystemContext context)
        {
            _context = context;
        }

        // =============================
        // CONTACT LIST VIEW
        // =============================
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts
                .Include(c => c.ClientContacts)
                .ThenInclude(cc => cc.Client)
                .OrderBy(c => c.Surname)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(contacts);
        }

        // =============================
        // CREATE CONTACT (GET)
        // =============================
        public IActionResult Create()
        {
            var vm = new ContactCreateViewModel
            {
                Clients = _context.Clients
                    .OrderBy(c => c.Name)
                    .ToList()
            };

            return View(vm);
        }

        // =============================
        // CREATE CONTACT (POST)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Clients = _context.Clients
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(vm);
            }

            // Friendly uniqueness validation for Email
            var emailNormalized = (vm.Email ?? string.Empty).Trim().ToLowerInvariant();
            var emailExists = await _context.Contacts
                .AnyAsync(c => c.Email.ToLower() == emailNormalized);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.Email), "A contact with this email address already exists.");
                vm.Clients = _context.Clients
                    .OrderBy(c => c.Name)
                    .ToList();
                return View(vm);
            }

            var contact = new Contact
            {
                Name = vm.Name,
                Surname = vm.Surname,
                Email = vm.Email
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            if (vm.SelectedClients != null)
            {
                foreach (var clientId in vm.SelectedClients)
                {
                    _context.ClientContacts.Add(new ClientContact
                    {
                        ClientId = clientId,
                        ContactId = contact.ContactId   // ✅ Correct PK name
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // EDIT CONTACT (GET)
        // =============================
        public async Task<IActionResult> Edit(int id)
        {
            var contact = await _context.Contacts
                .Include(c => c.ClientContacts)
                .ThenInclude(cc => cc.Client)
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
                return NotFound();

            var linkedClientIds = contact.ClientContacts
                .Select(cc => cc.ClientId)
                .ToHashSet();

            var vm = new ContactEditViewModel
            {
                ContactId = contact.ContactId,
                Name = contact.Name,
                Surname = contact.Surname,
                Email = contact.Email,
                LinkedClientContacts = contact.ClientContacts
                    .OrderBy(cc => cc.Client.Name)
                    .ToList(),
                AvailableClients = await _context.Clients
                    .Where(c => !linkedClientIds.Contains(c.ClientId))
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        // =============================
        // EDIT CONTACT (POST)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactEditViewModel vm)
        {
            if (id != vm.ContactId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Re-hydrate lists for redisplay
                var currentLinks = await _context.ClientContacts
                    .Where(cc => cc.ContactId == id)
                    .Include(cc => cc.Client)
                    .ToListAsync();

                var linkedClientIds = currentLinks.Select(cc => cc.ClientId).ToHashSet();

                vm.LinkedClientContacts = currentLinks.OrderBy(cc => cc.Client.Name).ToList();
                vm.AvailableClients = await _context.Clients
                    .Where(c => !linkedClientIds.Contains(c.ClientId))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(vm);
            }

            // Friendly uniqueness validation for Email (excluding this contact)
            var emailNormalized = (vm.Email ?? string.Empty).Trim().ToLowerInvariant();
            var emailExists = await _context.Contacts
                .AnyAsync(c => c.ContactId != id && c.Email.ToLower() == emailNormalized);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.Email), "A contact with this email address already exists.");

                var currentLinks = await _context.ClientContacts
                    .Where(cc => cc.ContactId == id)
                    .Include(cc => cc.Client)
                    .ToListAsync();

                var linkedClientIds = currentLinks.Select(cc => cc.ClientId).ToHashSet();
                vm.LinkedClientContacts = currentLinks.OrderBy(cc => cc.Client.Name).ToList();
                vm.AvailableClients = await _context.Clients
                    .Where(c => !linkedClientIds.Contains(c.ClientId))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(vm);
            }

            var existingContact = await _context.Contacts.FindAsync(id);

            if (existingContact == null)
                return NotFound();

            // ✅ Update editable fields only
            existingContact.Name = vm.Name;
            existingContact.Surname = vm.Surname;
            existingContact.Email = vm.Email;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // LINK CLIENT TO CONTACT (URL / FORM POST)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkClient(int contactId, int clientId)
        {
            var contactExists = await _context.Contacts.AnyAsync(c => c.ContactId == contactId);
            var clientExists = await _context.Clients.AnyAsync(c => c.ClientId == clientId);

            if (!contactExists || !clientExists)
                return NotFound();

            var linkExists = await _context.ClientContacts
                .AnyAsync(cc => cc.ContactId == contactId && cc.ClientId == clientId);

            if (!linkExists)
            {
                _context.ClientContacts.Add(new ClientContact
                {
                    ContactId = contactId,
                    ClientId = clientId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = contactId });
        }

        // =============================
        // UNLINK CLIENT FROM CONTACT (URL / FORM POST)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkClient(int contactId, int clientId)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId);

            if (link != null)
            {
                _context.ClientContacts.Remove(link);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = contactId });
        }

        // =============================
        // DELETE CONTACT
        // =============================
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts
                .Include(c => c.ClientContacts)
                .FirstOrDefaultAsync(c => c.ContactId == id);  // ✅ Fixed PK name

            if (contact == null)
                return NotFound();

            if (contact.ClientContacts.Any())
            {
                TempData["Error"] = "Cannot delete contact linked to a client.";
                return RedirectToAction(nameof(Index));
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // UNLINK CONTACT FROM CLIENT (AJAX)
        // =============================
        [HttpPost]
        public async Task<IActionResult> UnlinkClientAjax([FromBody] UnlinkClientDto dto)
        {
            var contactId = dto.ContactId;
            var clientId = dto.ClientId;

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

        public class UnlinkClientDto
        {
            public int ContactId { get; set; }
            public int ClientId { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripApi.Context;
using TripApi.Models;

namespace TripApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MasterContext _context;

        public ClientsController(MasterContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = _context.Clients.ToList();
            return Ok(clients);
        }

        // GET: api/Clients/{idClient}
        [HttpGet("{idClient}")]
        public IActionResult GetClient(int idClient)
        {
            var client = _context.Clients
                                 .Include(c => c.ClientTrips)
                                 .ThenInclude(ct => ct.IdTripNavigation)
                                 .FirstOrDefault(c => c.IdClient == idClient);

            if (client == null)
            {
                return NotFound("Client not found.");
            }

            return Ok(client);
        }

        // POST: api/Clients
        [HttpPost]
        public IActionResult CreateClient([FromBody] Client newClient)
        {
            if (newClient == null)
            {
                return BadRequest("Invalid client data.");
            }

            var existingClient = _context.Clients.FirstOrDefault(c => c.Pesel == newClient.Pesel);
            if (existingClient != null)
            {
                return BadRequest("Client with the same PESEL already exists.");
            }

            _context.Clients.Add(newClient);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetClient), new { idClient = newClient.IdClient }, newClient);
        }

        // PUT: api/Clients/{idClient}
        [HttpPut("{idClient}")]
        public IActionResult UpdateClient(int idClient, [FromBody] Client updatedClient)
        {
            if (updatedClient == null || idClient != updatedClient.IdClient)
            {
                return BadRequest("Invalid client data.");
            }

            var client = _context.Clients.Find(idClient);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            client.FirstName = updatedClient.FirstName;
            client.LastName = updatedClient.LastName;
            client.Email = updatedClient.Email;
            client.Telephone = updatedClient.Telephone;
            client.Pesel = updatedClient.Pesel;

            _context.Clients.Update(client);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/Clients/{idClient}
        [HttpDelete("{idClient}")]
        public IActionResult DeleteClient(int idClient)
        {
            var client = _context.Clients.Find(idClient);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            _context.Clients.Remove(client);
            _context.SaveChanges();

            return NoContent();
        }
    }
}

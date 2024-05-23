using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripApi.Context;
using TripApi.Models;

namespace TripApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly MasterContext _context;

        public TripsController(MasterContext context)
        {
            _context = context;
        }

        // GET: api/Trips
        [HttpGet]
        public IActionResult GetTrips()
        {
            var trips = _context.Trips
                                .Include(t => t.ClientTrips)
                                .ThenInclude(ct => ct.IdClientNavigation)
                                .OrderByDescending(t => t.DateFrom)
                                .ToList();

            var result = trips.Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Clients = t.ClientTrips.Select(ct => new
                {
                    ct.IdClientNavigation.FirstName,
                    ct.IdClientNavigation.LastName
                })
            });

            return Ok(result);
        }

        // POST: api/Trips/{idTrip}/clients
        [HttpPost("{idTrip}/clients")]
        public IActionResult AssignClientToTrip(int idTrip, [FromBody] Client assignClient)
        {
            if (assignClient == null)
            {
                return BadRequest("Invalid client data.");
            }

            var trip = _context.Trips.Find(idTrip);
            if (trip == null)
            {
                return NotFound("Trip not found.");
            }

            var client = _context.Clients.FirstOrDefault(c => c.Pesel == assignClient.Pesel);
            if (client == null)
            {
                client = new Client
                {
                    FirstName = assignClient.FirstName,
                    LastName = assignClient.LastName,
                    Email = assignClient.Email,
                    Telephone = assignClient.Telephone,
                    Pesel = assignClient.Pesel
                };
                _context.Clients.Add(client);
                _context.SaveChanges();
            }

            var clientTrip = _context.ClientTrips.FirstOrDefault(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);
            if (clientTrip != null)
            {
                return BadRequest("Client is already assigned to this trip.");
            }

            clientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = assignClient.ClientTrips.FirstOrDefault()?.PaymentDate ?? DateTime.Now
            };

            _context.ClientTrips.Add(clientTrip);
            _context.SaveChanges();

            return NoContent();
        }
    }
}

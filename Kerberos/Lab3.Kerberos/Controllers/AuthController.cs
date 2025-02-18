using Lab3.Kerberos.Data;
using Lab3.Kerberos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Lab3.Kerberos.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly KerberosDbContext _kerberosContext;

        public AuthController(KerberosDbContext kerberosContext)
        {
            _kerberosContext = kerberosContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (_kerberosContext.Users.Any(u => u.Name == request.Name))
            {
                return BadRequest("User already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Password = HashPassword(request.Password)
            };

            _kerberosContext.Users.Add(user);
            await _kerberosContext.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _kerberosContext.Users.FirstOrDefaultAsync(u => u.Name == request.Name);
            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            // Создаем TGT
            var tgt = new KerberosTicket
            {
                Name = user.Name,
                Ticket = GenerateTicket(),
                Expiration = DateTime.UtcNow.AddMinutes(30) 
            };

            _kerberosContext.Tickets.Add(tgt);
            await _kerberosContext.SaveChangesAsync();

            return Ok(new { TGT = tgt.Ticket, Expiration = tgt.Expiration });
        }

        [HttpPost("request-service-ticket")]
        public async Task<IActionResult> RequestServiceTicket([FromBody] ServiceTicketRequest request)
        {
            var tgt = await _kerberosContext.Tickets.FirstOrDefaultAsync(t => t.Ticket == request.TGT);
            if (tgt == null || tgt.Expiration < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired TGT");
            }

            var st = new KerberosTicket
            {
                Name = tgt.Name,
                Ticket = GenerateTicket(),
                Expiration = DateTime.UtcNow.AddMinutes(15) 
            };

            _kerberosContext.Tickets.Add(st);
            await _kerberosContext.SaveChangesAsync();

            return Ok(new { ServiceTicket = st.Ticket, Expiration = st.Expiration });
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash == storedHash;
        }

        private string GenerateTicket()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}

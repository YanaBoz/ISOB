using Microsoft.AspNetCore.Mvc;
using Lab3.API.Data;
using Lab3.API.Models;
using Lab3.Kerberos.Data;
using Microsoft.EntityFrameworkCore;

namespace Lab3.API.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly KerberosDbContext _kerberosContext;

        public BookController(AppDbContext context, KerberosDbContext kerberosContext)
        {
            _context = context;
            _kerberosContext = kerberosContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var ticket = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(ticket))
            {
                return Unauthorized("Missing Kerberos Service Ticket");
            }

            var validTicket = await _kerberosContext.Tickets
                .FirstOrDefaultAsync(t => t.Ticket == ticket && t.Expiration > DateTime.UtcNow);

            if (validTicket == null)
            {
                return Unauthorized("Invalid or expired Kerberos ticket");
            }

            var books = await _context.Books.ToListAsync();
            return Ok(books);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            var ticket = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(ticket))
            {
                return Unauthorized("Missing Kerberos Service Ticket");
            }

            var validTicket = await _kerberosContext.Tickets
                .FirstOrDefaultAsync(t => t.Ticket == ticket && t.Expiration > DateTime.UtcNow);

            if (validTicket == null)
            {
                return Unauthorized("Invalid or expired Kerberos ticket");
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return Ok(book);
        }
    }
}

namespace Lab3.Kerberos.Models
{
    public class LoginResponse
    {
        public string Ticket { get; set; }
        public DateTime Expiration { get; set; }
    }
}

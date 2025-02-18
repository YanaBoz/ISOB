namespace Lab3.Kerberos.Models
{
    public class KerberosTicket
    {
        public int Id { get; set; }
        public string Name { get; set; }  
        public string Ticket { get; set; }  
        public DateTime Expiration { get; set; }  
        public bool IsServiceTicket { get; set; }  
    }
}

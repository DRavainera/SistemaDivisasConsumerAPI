namespace SistemaDivisasConsumerAPI.Models
{
    public interface ICuenta
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public double Saldo { get; set; }
    }
}

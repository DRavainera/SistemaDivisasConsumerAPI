namespace SistemaDivisasConsumerAPI.Models
{
    public class CuentaPeso : ICuenta
    {
        public int Id { get; set; }
        public int NumCuenta { get; set; }
        public int IdCliente { get; set; }
        public int CBU { get; set; }
        public string AliasCBU { get; set; }
        public double Saldo { get; set; }
    }
}

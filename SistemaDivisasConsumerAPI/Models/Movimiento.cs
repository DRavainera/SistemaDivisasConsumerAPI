namespace SistemaDivisasConsumerAPI.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        public string NumCuenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
    }
}

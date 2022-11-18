using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaDivisasConsumerAPI.Data
{
    public class ReminderViewModel
    {
        public int Id { get; set; }
        public int NumCuenta { get; set; }
        public int IdCliente { get; set; }
        public int CBU { get; set; }
        public string AliasCBU { get; set; }
        public double Saldo { get; set; }
    }
}

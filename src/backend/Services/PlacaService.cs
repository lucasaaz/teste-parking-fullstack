using System.Text.RegularExpressions;

namespace Parking.Api.Services
{
    public class PlacaService
    {
        public string Sanitizar(string? placa)
        {
            return Regex.Replace(placa ?? "", "[^A-Za-z0-9]", "").ToUpperInvariant();
        }

        public bool EhValida(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");
        }
    }
}
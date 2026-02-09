
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Models;
using Parking.Api.Services;
using System.Text;

namespace Parking.Api.Controllers
{
    [ApiController]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PlacaService _placa;
        public ImportController(AppDbContext db, PlacaService placa) { _db = db; _placa = placa; }

        [HttpPost("csv")]
        public async Task<IActionResult> ImportCsv()
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                return BadRequest("Envie um arquivo CSV no campo 'file'.");

            var file = Request.Form.Files[0];
            using var s = file.OpenReadStream();
            using var r = new StreamReader(s, Encoding.UTF8);

            int linha = 0, processados = 0, inseridos = 0;
            var erros = new List<string>();
            string? header = await r.ReadLineAsync(); // consome cabeçalho
            while (!r.EndOfStream)
            {
                linha++;
                var raw = await r.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(raw)) continue;
                processados++;

                try
                {
                    // CSV simples separado por vírgula: placa,modelo,ano,cliente_identificador,cliente_nome,cliente_telefone,cliente_endereco,mensalista,valor_mensalidade
                    var cols = raw.Split(',');
                    if (cols.Length < 9) throw new Exception("Colunas insuficientes. Esperado: 9.");

                    var placa = _placa.Sanitizar(cols[0]);
                    var modelo = cols[1];
                    var ano = int.TryParse(cols[2], out var a) ? a : (int?)null;
                    var cliNome = cols[4];
                    var cliTel = new string((cols[5] ?? "").Where(char.IsDigit).ToArray());
                    var cliEnd = cols[6];
                    bool mensalista = bool.TryParse(cols[7], out var m) && m;
                    decimal? valorMens = decimal.TryParse(cols[8], out var vm) ? vm : null;

                    if (!_placa.EhValida(placa)) throw new Exception($"A placa '{placa}' é inválida.");
                    if (await _db.Veiculos.AnyAsync(v => v.Placa == placa)) throw new Exception($"A placa '{placa}' já existe no sistema.");

                    var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Nome == cliNome && c.Telefone == cliTel);
                    if (cliente == null)
                    {
                        cliente = new Cliente { Nome = cliNome, Telefone = cliTel, Endereco = cliEnd, Mensalista = mensalista, ValorMensalidade = valorMens };
                        _db.Clientes.Add(cliente);
                        await _db.SaveChangesAsync();
                    }

                    var v = new Veiculo { Placa = placa, Modelo = modelo, Ano = ano, ClienteId = cliente.Id };
                    _db.Veiculos.Add(v);
                    await _db.SaveChangesAsync();
                    inseridos++;
                }
                catch (Exception ex)
                {
                    erros.Add($"Linha {linha}: {ex.Message}");
                }
            }

            return Ok(new { processados, inseridos, erros });
        }
    }
}

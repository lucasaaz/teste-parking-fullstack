using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Models;

namespace Parking.Api.Services
{
    public class FaturamentoService
    {
        private readonly AppDbContext _db;
        public FaturamentoService(AppDbContext db) => _db = db;

        public async Task<List<Fatura>> GerarAsync(string competencia, CancellationToken ct = default)
        {
            var part = competencia.Split('-');
            var ano = int.Parse(part[0]);
            var mes = int.Parse(part[1]);
            
            var inicioMes = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);
            decimal diasNoMes = DateTime.DaysInMonth(ano, mes);

            var mensalistas = await _db.Clientes.Where(c => c.Mensalista).AsNoTracking().ToListAsync(ct);
            var criadas = new List<Fatura>();

            foreach (var cli in mensalistas)
            {
                if (await _db.Faturas.AnyAsync(f => f.ClienteId == cli.Id && f.Competencia == competencia, ct)) continue;

                var veiculos = await _db.Veiculos.Where(v => v.ClienteId == cli.Id).ToListAsync(ct);
                
                var fat = new Fatura {
                    Id = Guid.NewGuid(),
                    Competencia = competencia,
                    ClienteId = cli.Id,
                    Valor = 0,
                    Observacao = "Faturamento proporcional por dias de posse."
                };

                decimal totalFatura = 0;
                foreach (var v in veiculos)
                {
                    // Lógica Proporcional
                    DateTime inicioCobranca = v.DataInclusao > inicioMes ? v.DataInclusao : inicioMes;
                    if (inicioCobranca > fimMes) continue;

                    int diasAtivos = (fimMes.Date - inicioCobranca.Date).Days + 1;
                    decimal valorDiario = (cli.ValorMensalidade ?? 0) / diasNoMes;
                    totalFatura += valorDiario * diasAtivos;

                    fat.Veiculos.Add(new FaturaVeiculo { FaturaId = fat.Id, VeiculoId = v.Id });
                }

                if (totalFatura > 0)
                {
                    fat.Valor = Math.Round(totalFatura, 2);
                    _db.Faturas.Add(fat);
                    criadas.Add(fat);
                }
            }

            await _db.SaveChangesAsync(ct);
            return criadas;
        }
    }
}
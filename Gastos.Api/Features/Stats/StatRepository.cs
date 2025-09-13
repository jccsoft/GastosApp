namespace Gastos.Api.Features.Stats;

public class StatRepository(AppDbContext context) : IStatRepository
{
    public async Task<List<Stat>> GetStatsAsync(string userId, StatParameters parameters, CancellationToken token)
    {
        if (parameters.DateStartUtc > parameters.DateEndUtc)
            return [];

        var filteredReceipts = context.Receipts
            .Where(r => r.UserId == userId &&
                        r.TransactionDateUtc >= parameters.DateStartUtc &&
                        r.TransactionDateUtc <= parameters.DateEndUtc);

        var groupedStats = GroupAndSelectByPeriod(filteredReceipts, parameters.Period);

        var orderedStats = groupedStats.OrderBy(s => s.Date);


        if (parameters.Period == StatType.Weekly)
            return [.. orderedStats];


        return await orderedStats.ToListAsync(token);
    }

    private static IQueryable<Stat> GroupAndSelectByPeriod(IQueryable<Receipt> receipts, StatType period)
    {
        if (period == StatType.Weekly)
        {
            // Materializa los datos para evitar problemas de agrupación en la base de datos
            var receiptAmounts = receipts
                .Select(r => new
                {
                    r.TransactionDateUtc,
                    Amounts = r.Items.Select(item => item.Amount).ToList()
                })
                .AsEnumerable();

            // Agrupa por semana ISO
            return receiptAmounts
                .GroupBy(x => new
                {
                    Year = ISOWeek.GetYear(x.TransactionDateUtc!.Value),
                    Week = ISOWeek.GetWeekOfYear(x.TransactionDateUtc!.Value)
                })
                .Select(g => new Stat
                {
                    Date = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                    Amount = g.SelectMany(x => x.Amounts).Sum()
                })
                .AsQueryable();
        }


        return period switch
        {
            StatType.Monthly => receipts
                .GroupBy(r => new { r.TransactionDateUtc!.Value.Year, r.TransactionDateUtc.Value.Month })
                .Select(g => new Stat
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    Amount = g.SelectMany(r => r.Items).Sum(i => i.Amount)
                }),

            _ => receipts
                .GroupBy(r => r.TransactionDateUtc!.Value.Date)
                .Select(g => new Stat
                {
                    Date = g.Key,
                    Amount = g.SelectMany(r => r.Items).Sum(i => i.Amount)
                })
        };
    }
}
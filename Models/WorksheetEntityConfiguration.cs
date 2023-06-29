using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace LearnApi.Models;

public class WorksheetEntityConfiguration : IEntityTypeConfiguration<Worksheet>
{
    public void Configure(EntityTypeBuilder<Worksheet> builder)
    {
        builder.Property(w => w.Exercises)
            .HasConversion(
                a => (string)JsonConvert.SerializeObject(a),
                a => JsonConvert.DeserializeObject<List<List<Symbol>>>(a)!);
    }
}
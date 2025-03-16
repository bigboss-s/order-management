using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace OrderManagement.Entities.Configs
{
    public class ClientEfConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(e => e.Id).HasName("Client_pk");
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.ClientType).IsRequired();
            builder.Property(e => e.Address).IsRequired().HasMaxLength(100);

            builder.ToTable(nameof(Client));

            Client[] clients = new Client[]
            {
            new Client { Id = 1, Name = "Mr. White", ClientType = ClientType.Individual, Address = "308 Negra Arroyo Lane, Albuquerque, NM"},
            new Client { Id = 2, Name = "Los Pollos Hermoanos", ClientType = ClientType.Company, Address = "4275 Isleta Blvd SW, Albuquerque, NM "}
            };

            builder.HasData(clients);
        }
    }
}
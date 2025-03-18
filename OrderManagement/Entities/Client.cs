
namespace OrderManagement.Entities
{
    public enum ClientType
    {
        Company,
        Individual
    }

    public class Client
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ClientType ClientType { get; set; }
        public required string Address { get; set; }
        public virtual ICollection<Order> Orders {get; set;} = new List<Order>();

        public override string ToString()
        {
            return Id + " " + Name + ", at "+ Address;
        }
    }

}
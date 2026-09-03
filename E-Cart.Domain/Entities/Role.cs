using E_Cart.Domain.Common;

namespace ECart.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Navigation property: A role has many users
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
using SimpleInventory.Common.Classes;
using SimpleInventory.DataAccess.DAOs;
using SimpleInventory.DataAccess.DTOs;

namespace SimpleInventory.DataAccess.Classes
{
    public static class MappingHelper
    {
        public static CategoryDAO DomainToDao(this Category d) => new()
        {
            Id = d.Id,
            Name = d.Name
        };
        public static Category DaoToDomain(this CategoryDAO d) => new()
        {
            Id = d.Id,
            Name = d.Name
        };
        public static ProductDAO DomainToDao(this Product d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Sku = d.Sku,
            CategoryId = d.CategoryId,
            Price = d.Price,
            Quantity = d.Quantity
            

        };
        public static Product DaoToDomain(this ProductDAO d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Sku = d.Sku,
            CategoryId = d.CategoryId,
            Price = d.Price,
            Quantity = d.Quantity
        };
        public static ProductDTO ToDto(this Product p) => new()
        {
            Sku = p.Sku,
            Name = p.Name,
            Price = p.Price,
            Quantity = p.Quantity,
            CategoryId = p.CategoryId,
            UpdatedAt = p.UpdateAt
        };

        public static CategoryDTO ToDto(this Category c) => new()
        {
            Name = c.Name
        };
    }
}

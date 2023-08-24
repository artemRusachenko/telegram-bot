 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineShopBot.Models
{
    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }

        static async Task<List<Product>> GetProducts()
        {
            using (FileStream fs = new FileStream("../../../files/products.json", FileMode.OpenOrCreate))
            {
                List<Product> products = await JsonSerializer.DeserializeAsync<List<Product>>(fs);
                return products;
            }
        }

        public static async Task<List<Product>> GetProductsByCategoryId(int categoryId)
        {
            List<Product> products = await GetProducts();
            List<Product> chosen = new List<Product>();
            foreach(Product product in products)
            {
                if (categoryId == product.CategoryId && product.Quantity > 0)
                    chosen.Add(product);
            }
            return chosen;
        }

        public static async Task<Product> GetProductById(int id)
        {
            List<Product> products = await GetProducts();
            foreach (var prod in products)
            {
                if (id == prod.Id)
                    return prod;
            }
            throw new Exception("Product with this id doesn't exist");
        }

    }
}
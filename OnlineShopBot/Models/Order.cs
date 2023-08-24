using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShopBot.Models
{
    internal class Order
    {        
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<int, int> Products { get; set; }
        public long UserId { get; set; }
        public bool IsConfirmed { get; set; }

        public Order(int id, long userId)
        {
            Id = id;
            Date = DateTime.Now;
            Products = new Dictionary<int, int>();
            IsConfirmed = false;
            UserId = userId;
        }

        async public void AddToOrder(int prodId)
        {
            if (Products.ContainsKey(prodId))
            {
                Products[prodId]++;
            }
            else
            {
                Products.Add(prodId, 1);
            }
            List<Product> products = null;
            using (FileStream fs = new FileStream("../../../files/products.json", FileMode.OpenOrCreate))
            {
                products = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Product>>(fs);
            }
            foreach (Product p in products)
            {
                if(p.Id == prodId)
                {
                    p.Quantity -= 1;
                }
            }
            var productsJson = JsonConvert.SerializeObject(products);
            await System.IO.File.WriteAllTextAsync("../../../files/products.json", productsJson);
        }

        public async Task<string> ToString()
        {
            string info = "";
            decimal sum = 0;
            foreach (var item in Products)
            {
                var p = await Product.GetProductById(item.Key);
                info += $"{p.Name}\nPrice: {p.Price}\nQuantity: {item.Value}\n";
                sum += p.Price * item.Value;
            }
            info += $"\nTotal sum: {sum}";
            return info;
        }

        public async Task<string> GetInfoForAdmins()
        {
            string info = "";
            decimal sum = 0;
            List<BotUser> users = await BotSystem.GetUsers();
            foreach (BotUser user in users)
            {
                if(user.Id == UserId)
                {
                    info += $"{user.FirstName} {user.LastName}\nPhone number: {user.PhoneNumber}\n";
                }
            }
            foreach (var item in Products)
            {
                var p = await Product.GetProductById(item.Key);
                info += $"{p.Name}\nPrice: {p.Price}\nQuantity: {item.Value}\n";
                sum += p.Price * item.Value;
            }
            info += $"\nTotal sum: {sum}";
            return info;
        }
    }
}

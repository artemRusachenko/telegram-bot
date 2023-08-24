using Newtonsoft.Json;
using OnlineShopBot.Buttons;
using OnlineShopBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OnlineShopBot
{
    static class BotSystem
    {
        public static int Count = 0;
        public static List<Order> orders = new List<Order>();
        public static List<BotUser> users = new List<BotUser>();

        public static async Task<List<BotUser>> GetUsers()
        {
            using (FileStream fs = new FileStream("../../../files/users.json", FileMode.OpenOrCreate))
            {
                users = await System.Text.Json.JsonSerializer.DeserializeAsync<List<BotUser>>(fs);
                return users;
            }
        }

        public static async Task<List<Order>> GetOrders()
        {
            using (FileStream fs = new FileStream("../../../files/orders.json", FileMode.OpenOrCreate))
            {
                orders = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Order>>(fs);
                return orders;
            }
        }

        public static async Task<string> GetOrderInfo(long userId)
        {
            foreach (Order ord in orders)
            {
                if (ord.UserId == userId && ord.IsConfirmed == false)
                {
                    return await ord.ToString();
                }
            }
            return "Your basket is empty";
        }

        public static async Task<List<Order>> GetOrdersByUserId(long userId)
        {
            using (FileStream fs = new FileStream("../../../files/orders.json", FileMode.OpenOrCreate))
            {
                List<Order> orders = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Order>>(fs);
                List<Order> userOrders = new List<Order>();
                foreach (Order or in orders)
                {
                    if (or.UserId == userId)
                        userOrders.Add(or);
                }

                return userOrders;
            }
        }

        public static void AddOrder(Order order) => orders.Add(order);

        async public static Task<int> ConfirmTheOrder(long userId)
        {
            int orderID = -1;
            foreach (Order ord in orders)
            {
                if (ord.UserId == userId && ord.IsConfirmed == false)
                {
                    ord.IsConfirmed = true;
                    orderID = ord.Id;
                }
            }
            var ordersJson = JsonConvert.SerializeObject(orders);
            await System.IO.File.WriteAllTextAsync("../../../files/orders.json", ordersJson);
            return orderID;
        }

        async public static Task RejectTheOrder(long userId)
        {
            List<Product> products = null;
            using (FileStream fs = new FileStream("../../../files/products.json", FileMode.OpenOrCreate))
            {
                products = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Product>>(fs);
            }
            Order rejectedOrder = null;            
            foreach (Order ord in orders)
            {
                if (ord.UserId == userId && ord.IsConfirmed == false)
                {
                    rejectedOrder = ord;
                }
            }
            orders.Remove(rejectedOrder);

            var ordersJson = JsonConvert.SerializeObject(orders);
            await System.IO.File.WriteAllTextAsync("../../../files/orders.json", ordersJson);
            foreach (Product p in products)
            {
                foreach (var pr in rejectedOrder.Products)
                {
                    if (pr.Key == p.Id)
                    {
                        p.Quantity += pr.Value;
                    }
                }
            }
            var productsJson = JsonConvert.SerializeObject(products);
            await System.IO.File.WriteAllTextAsync("../../../files/products.json", productsJson);
        }

        public static async Task<bool> ProductisAvailable(int productID)
        {
            List<Product> products = null;
            using (FileStream fs = new FileStream("../../../files/products.json", FileMode.OpenOrCreate))
            {
                products = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Product>>(fs);
            }
            foreach(Product p in products)
            {
                if(p.Id == productID && p.Quantity > 0) return true;
            }
            return false;
        }

        public static async Task SendOrderToAdmin(ITelegramBotClient botClient, Update update, CancellationToken token, int orderID)
        {
            string msg = "";
            List<BotUser> admins = null;
            List<Order> orders = await GetOrders();
            foreach(Order order in orders)
            {
                if(order.Id == orderID)
                {
                    msg = await order.GetInfoForAdmins();
                }
            }
            using (FileStream fs = new FileStream("../../../files/admins.json", FileMode.OpenOrCreate))
            {
                admins = await System.Text.Json.JsonSerializer.DeserializeAsync<List<BotUser>>(fs);
            }
            foreach(BotUser admin in admins)
            {
                await botClient.SendTextMessageAsync(
                chatId: admin.Id  ,
                text: msg,
                cancellationToken: token
                );
            }
            /*
            await botClient.SendTextMessageAsync(
                chatId: userId,
                text: msg,
                cancellationToken: token
                );
            */
        }
    }
}
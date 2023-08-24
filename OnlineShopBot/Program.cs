using Newtonsoft.Json;
using OnlineShopBot;
using OnlineShopBot.Buttons;
using OnlineShopBot.Constans;
using OnlineShopBot.Models;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

List<string> namesMainCategories = await Category.GetCategoriesByParentId(0);
List<Category> categories = await Category.GetCategoriesAsync();
Order order = null;

TelegramBotClient botClient = new TelegramBotClient(Constant.TELEGRAM_TOKEN);
CancellationTokenSource cts = new CancellationTokenSource();
ReceiverOptions receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandleErrorPollingAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
    );

Console.WriteLine("Bot is working");
Console.ReadLine();

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
{    
    if (update.Type != UpdateType.Message && update.Type != UpdateType.CallbackQuery)
        return;    
    if (update.Message?.Type == MessageType.Text)
    {
        string? messageText = update.Message?.Text;
        long id = update.Message.Chat.Id;
        string? user = update.Message?.From?.FirstName;

        if (messageText == "/start")
        {
            await SendMessageAsync(botClient, id, $"Hi, {user}!", Button.startBtns, null, false, token);
        }
        else if (messageText == Button.infoBtn.Text)
        {
            await SendMessageAsync(botClient, id, $"We are an online telegram shop.\nHere you can find a lot of equipment\nat the lowest prices.", null, null, false, token);
        }
        else if (messageText == Button.helpBtn.Text)
        {
            await SendMessageAsync(botClient, id, $"after pressing the button (All products), you will see the assortment of our store.\nTo order in the online store, you need to register. you can register using the /registration command. If you still have questions, write to @optimiiiist", null, null, false, token);
        }
        else if (messageText == Button.catalogBtn.Text || messageText == Button.backBtn.Text)
        {
            KeyboardButton[] mainCategories = Button.GetCategoriesBtn(namesMainCategories);
            ReplyKeyboardMarkup rkm = Button.GetButtonPanel(mainCategories, Button.secondPannel);
            await SendMessageAsync(botClient, id, "Choose category", rkm, null, false, token);
        }
        else if (namesMainCategories.Contains(messageText))
        {
            int categoryId = -1;
            foreach (Category category in categories)
            {
                if (category.Name == messageText)
                {
                    categoryId = category.Id;
                }
            }
            List<string> namesSubCategories = await Category.GetCategoriesByParentId(categoryId);
            KeyboardButton[] subCategories = Button.GetCategoriesBtn(namesSubCategories);
            ReplyKeyboardMarkup rkm = Button.GetButtonPanel(subCategories, Button.defaultPannel);
            await SendMessageAsync(botClient, id, "Choose category", rkm, null, false, token);

        }
        else if (await Category.GetIdByName(messageText, categories) > 0)
        {            
            int catId = await Category.GetIdByName(messageText, categories);
            List<Product> products = await Product.GetProductsByCategoryId(catId);
            foreach (Product product in products)
            {
                await botClient.SendPhotoAsync(
                    chatId: id,
                    photo: product.Image,
                    replyMarkup: Button.CreateInlineKeyboard(product.Id.ToString()),
                    caption: $"{product.Name}\n" +
                    $"Price: {product.Price}\n" +
                    $"Characteristics: {product.Description}",
                    cancellationToken: token
                    );
            }
        }
        else if(messageText == Button.basketBtn.Text)
        {
            string msg = await BotSystem.GetOrderInfo(id);
            ReplyKeyboardMarkup rkm = null;
            if (msg == "Your basket is empty")
            {
                KeyboardButton[] mainCategories = Button.GetCategoriesBtn(namesMainCategories);
                rkm = Button.GetButtonPanel(mainCategories, Button.secondPannel);
            }
            else
            {
                rkm = Button.basketRkm;
            }
            await SendMessageAsync(botClient, id, msg, rkm, null, false, token);            
        }
        else if(messageText == Button.cnfBtn.Text)
        {
            await SendMessageAsync(botClient, id, "Your order is confirmed!\nOur manager will contact you soon", Button.startBtns, null, false, token);
            int orderID = await BotSystem.ConfirmTheOrder(id);            
            BotSystem.SendOrderToAdmin(botClient, update, token, orderID);
        }
        else if (messageText == Button.rjcBtn.Text)
        {
            await SendMessageAsync(botClient, id, "Your order is rejected", Button.startBtns, null, false, token);
            await BotSystem.RejectTheOrder(id);
        }
        else if(messageText == "/registration")
        {
            await botClient.SendTextMessageAsync(
            chatId: id,
            text: "Share your phone number",
            replyMarkup: Button.getPhoneNumber,
            cancellationToken: token
            );
        }
    }
    if (update.Message?.Type == MessageType.Contact)
    {
        long? userID = update.Message.Contact.UserId;
        List<BotUser> users = await BotSystem.GetUsers();
        BotUser user = users.FirstOrDefault(user => user.Id == userID);
        if (user == null)
        {
            string? firstName = update.Message.Contact.FirstName;
            string? lastName = update.Message.Contact.LastName;
            string number = update.Message.Contact.PhoneNumber;
            user = new BotUser(userID, number, firstName, lastName);
            users.Add(user);
            var usersJson = JsonConvert.SerializeObject(users);
            await System.IO.File.WriteAllTextAsync("../../../files/users.json", usersJson);
        }
        await SendMessageAsync(botClient, userID, "Registration completed successfully", Button.startBtns, null, false, token);
    }
    else if (update.CallbackQuery != null)
    {
        long userId = update.CallbackQuery.From.Id;
        List<BotUser> users = await BotSystem.GetUsers();
        BotUser user = users.FirstOrDefault(user => user.Id == userId);
        if (user == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: userId,
                text: "You need to register to place an order. Register using the /registration command",
                cancellationToken: token
                );
        }
        else if (await BotSystem.ProductisAvailable(int.Parse(update.CallbackQuery.Data)))
        {            
            

            List<Order> userOrders = await BotSystem.GetOrdersByUserId(userId);
            if (userOrders.Count == 0 || userOrders[userOrders.Count - 1].IsConfirmed)
            {
                order = new Order(++BotSystem.Count, userId);
                order.AddToOrder(int.Parse(update.CallbackQuery.Data));
                BotSystem.AddOrder(order);
            }
            else
            {
                foreach (Order ord in BotSystem.orders)
                {
                    if (ord.UserId == userId && ord.IsConfirmed == false)
                    {
                        ord.AddToOrder(int.Parse(update.CallbackQuery.Data));
                    }
                }
            }

            var ordersJson = JsonConvert.SerializeObject(BotSystem.orders);
            await System.IO.File.WriteAllTextAsync("../../../files/orders.json", ordersJson);

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                text: "This product added to your order",
                cancellationToken: token
            );
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: userId,
                text: "This product isn't available",
                cancellationToken: token
                );
        }       
    }
}

Task HandleErrorPollingAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
{
    string error = exception switch
    {
        ApiRequestException ex => $"Telegram API error: {ex.ErrorCode}\n{ex.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(error);

    return Task.CompletedTask;
}

static async Task SendMessageAsync(ITelegramBotClient botClient, long? chatId, string message, ReplyKeyboardMarkup btn, ParseMode? mode, bool notifications, CancellationToken token)
{
    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: message,
        parseMode: mode,
        replyMarkup: btn,
        disableNotification: notifications,
        cancellationToken: token
        );
}
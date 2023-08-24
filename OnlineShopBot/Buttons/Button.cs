using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace OnlineShopBot.Buttons
{
    static class Button
    {
        public static KeyboardButton backBtn = new KeyboardButton("⬅️ Back");
        public static KeyboardButton helpBtn = new KeyboardButton("🆘 Help");
        public static KeyboardButton infoBtn = new KeyboardButton("About us");
        public static KeyboardButton catalogBtn = new KeyboardButton("All products");
        public static KeyboardButton basketBtn = new KeyboardButton("🧺 Basket");
        public static KeyboardButton cnfBtn = new KeyboardButton("✅ Confirm");
        public static KeyboardButton rjcBtn = new KeyboardButton("❌ Reject");
        //Panels
        public static KeyboardButton[] startPannel = { infoBtn, helpBtn};
        public static KeyboardButton[] secondPannel = { basketBtn};
        public static KeyboardButton[] defaultPannel = { backBtn, basketBtn};
        public static ReplyKeyboardMarkup basketRkm = new ReplyKeyboardMarkup(new[] { backBtn, cnfBtn, rjcBtn })
        {
            ResizeKeyboard = true
        };
        public static ReplyKeyboardMarkup startBtns = GetButtonPanel(new KeyboardButton[] {catalogBtn}, startPannel);

        public static ReplyKeyboardMarkup getPhoneNumber = new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact("Share my phone number"))
        {
            ResizeKeyboard = true
        };


        public static InlineKeyboardMarkup CreateInlineKeyboard(string data)
        {
            return new(InlineKeyboardButton.WithCallbackData(text: "Buy", callbackData: data));
        }
        public static KeyboardButton[] GetCategoriesBtn(List<string> categoriesNames)
        {
            KeyboardButton[] categoriesBtns = new KeyboardButton[categoriesNames.Count];
            for(int i = 0; i < categoriesBtns.Length; i++)
            {
                categoriesBtns[i] = new KeyboardButton(categoriesNames[i]);
            }
            return categoriesBtns;
        }
        public static ReplyKeyboardMarkup GetButtonPanel(KeyboardButton[] row1, KeyboardButton[] row2)
        {
            ReplyKeyboardMarkup buttons = new ReplyKeyboardMarkup(new[]
            {
                row1,
                row2
            })
            {
                ResizeKeyboard = true
            };
            return buttons;
        }

    }
}

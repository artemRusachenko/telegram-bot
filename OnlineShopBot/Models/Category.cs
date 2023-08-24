using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineShopBot.Models
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public static async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> allCategories = new List<Category>();
            using (FileStream fs = new FileStream("../../../files/categories.json", FileMode.OpenOrCreate))
            {
                allCategories = await JsonSerializer.DeserializeAsync<List<Category>>(fs);
                return allCategories;
            }
        }
        public static async Task<List<string>> GetCategoriesByParentId(int parentId)
        {
            List<Category> categories = await GetCategoriesAsync();
            List<string> names = new List<string>();
            foreach (Category category in categories)
            {
                if (category.ParentId == parentId)
                    names.Add(category.Name);
            }
            return names;
        }
        public static async Task<int> GetIdByName(string name, List<Category> categories)
        {
            foreach(Category c in categories)
            {
                if(name == c.Name)
                {
                    return c.Id;
                }
            }
            return -1;
        }

    }
}


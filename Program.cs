﻿using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Display all records in the Products Table");
        Console.WriteLine("6) Edit a Record from the Products Table");
        Console.WriteLine("7) Edit record from the Categories table");
        Console.WriteLine("8) Display Categories and active products");
        Console.WriteLine("9) Delete a specified existing record from the Products table");
        Console.WriteLine("10) Delete a specified existing record from the Categories table");
        Console.WriteLine("11) Display all discontinued products.");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
                else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
                        ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    //save category to db
                    db.Categories.Add(category);
                    db.SaveChanges();
                    logger.Info("Category added - {name}", category.CategoryName);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
                else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (choice == "5")
        {
            Console.WriteLine("1) All Products");
            Console.WriteLine("2) Discontinued Products");
            Console.WriteLine("3) Active Products");
            var option = Console.ReadLine();

            var products = db.Products.AsQueryable();

            if (option == "2")
            {
                products = products.Where(p => p.Discontinued);
            }
            else if (option == "3")
            {
                products = products.Where(p => !p.Discontinued);
            }

            foreach (var product in products.OrderBy(p => p.ProductName))
            {
                if (product.Discontinued)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine(product.ProductName);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        else if (choice == "6")
        {
            Product product = new Product();
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter Supplier ID:");
            product.SupplierId = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter Category ID:");
            product.CategoryId = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter Quantity Per Unit:");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Unit Price:");
            product.UnitPrice = decimal.Parse(Console.ReadLine());
            Console.WriteLine("Enter Units In Stock:");
            product.UnitsInStock = short.Parse(Console.ReadLine());
            Console.WriteLine("Enter Units On Order:");
            product.UnitsOnOrder = short.Parse(Console.ReadLine());
            Console.WriteLine("Enter Reorder Level:");
            product.ReorderLevel = short.Parse(Console.ReadLine());
            Console.WriteLine("Is Discontinued (true/false):");
            product.Discontinued = bool.Parse(Console.ReadLine());

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                db.Products.Add(product);
                db.SaveChanges();
                logger.Info("Product added - {name}", product.ProductName);
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "7")
        {
            Console.WriteLine("Enter the Product ID to edit:");
            int id = int.Parse(Console.ReadLine());
            var product = db.Products.FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                Console.WriteLine("Enter new Product Name (leave blank to keep current):");
                var newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName)) product.ProductName = newName;

                // Repeat the above for other fields as necessary...
                // For brevity, only ProductName is being demonstrated here

                ValidationContext context = new ValidationContext(product, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(product, context, results, true);
                if (isValid)
                {
                    logger.Info("Validation passed");
                    db.SaveChanges();
                    logger.Info("Product edited - {name}", product.ProductName);
                }
                if (!isValid)
                {
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                }
            }
            else
            {
                logger.Error("Product ID not found");
            }
        }
        else if (choice == "8")
        {
            var query = db.Categories.Include(c => c.Products.Where(p => !p.Discontinued)).OrderBy(p => p.CategoryName);

            foreach (var category in query)
            {
                Console.WriteLine($"{category.CategoryName} - {category.Description}");
                foreach (var product in category.Products)
                {
                    Console.WriteLine($"\t{product.ProductName}");
                }
            }
        }
        else if (choice == "9")
{
    Console.WriteLine("Enter the Product ID to delete:");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var product = db.Products.Include(p => p.OrderDetails).FirstOrDefault(p => p.ProductId == id);

        if (product != null)
        {
            // Remove related order details first
            db.OrderDetails.RemoveRange(product.OrderDetails);

            // Now remove the product
            db.Products.Remove(product);

            db.SaveChanges();
            logger.Info("Product and related order details deleted - {name}", product.ProductName);
        }
        else
        {
            logger.Error("Product ID not found");
        }
    }
    else
    {
        logger.Error("Invalid Product ID entered");
    }
}
        else if (choice == "10")
        {
            Console.WriteLine("Enter the Category ID to delete:");
            int id = int.Parse(Console.ReadLine());
            var category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                db.Categories.Remove(category);
                db.SaveChanges();
                logger.Info("Category deleted - {name}", category.CategoryName);
            }
            else
            {
                logger.Error("Category ID not found");
            }
        }
        else if (choice == "11")
    {
        var discontinuedProducts = db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductName);

        if (discontinuedProducts.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Discontinued Products:");
            foreach (var product in discontinuedProducts)
            {
                Console.WriteLine(product.ProductName);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.WriteLine("There are no discontinued products.");
        }
    }
        Console.WriteLine();

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");
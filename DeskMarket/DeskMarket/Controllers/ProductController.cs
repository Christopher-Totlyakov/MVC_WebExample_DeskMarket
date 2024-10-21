using DeskMarket.Data;
using DeskMarket.Data.Models;
using DeskMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace DeskMarket.Controllers
{
    [Authorize]
    public class ProductController(ApplicationDbContext dbContext) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var product = new AddProductViewModel();
            product.Categories = await dbContext.Categories.AsNoTracking().ToArrayAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddProductViewModel productModel)
        {
            DateTime addedOn;
            if (DateTime.TryParseExact(productModel.AddedOn.Trim(), ComanConsts.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out addedOn) == false)
            {
                ModelState.AddModelError("addedOn", "Invalid date");
            }
            if (ModelState.IsValid == false)
            {
                productModel.Categories = await dbContext.Categories.AsNoTracking().ToArrayAsync();
                return View(productModel);
            }

            var product = new Product()
            {
                ProductName = productModel.ProductName,
                Description = productModel.Description,
                Price = productModel.Price,
                ImageUrl = productModel.ImageUrl,
                AddedOn = addedOn,
                CategoryId = productModel.CategoryId,
                SellerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                
            };

            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await dbContext.Products.Where(p => p.IsDeleted == false).Select(p => new IndexProductViewModel()
            {
                ImageUrl = p.ImageUrl,
                ProductName = p.ProductName,
                Price = p.Price,
                Id = p.Id,
                IsSeller = p.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier) ? true : false,
                HasBought = p.ProductsClients.Any(pc => pc.ClientId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? true : false
            })
              .AsNoTracking()
              .ToListAsync();



            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var product = await dbContext.ProductClients
                .Include(p => p.Product)
                .Where(pc => pc.Product.IsDeleted == false)
                .Where(pc => pc.ClientId == User.FindFirstValue(ClaimTypes.NameIdentifier)).Select(p => new CartProductViewModel()
                {
                    Id = p.Product.Id,
                    ImageUrl = p.Product.ImageUrl,
                    Price = p.Product.Price,
                    ProductName = p.Product.ProductName,

                }).AsNoTracking().ToListAsync();

            return View(product);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var product = await dbContext.Products
                .Where(p => p.IsDeleted == false && p.Id == id)
                .Select(p => new EditProductViewModel
                {
                    Description = p.Description,
                    AddedOn = p.AddedOn.ToString(ComanConsts.DateFormat),
                    CategoryId = p.CategoryId,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    ProductName = p.ProductName,
                    SellerId = p.SellerId,

                }).AsNoTracking().FirstOrDefaultAsync();

            if (product == null || product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction(nameof(Index));
            }

            product.Categories = await dbContext.Categories.AsNoTracking().ToListAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProductViewModel productModel)
        {
            if (productModel.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction(nameof(Index));
            }
                DateTime addedOn;
            if (DateTime.TryParseExact(productModel.AddedOn.Trim(), ComanConsts.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out addedOn) == false)
            {
                ModelState.AddModelError("addedOn", "Invalid date");
            }

            if (ModelState.IsValid == false)
            {
                productModel.Categories = await dbContext.Categories.AsNoTracking().ToArrayAsync();
                return View(productModel);
            }

            var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null || product.IsDeleted)
            {
                return View(productModel);
            }
            product.ProductName = productModel.ProductName;
            product.Price = productModel.Price;
            product.Description = productModel.Description;
            product.ImageUrl = productModel.ImageUrl;
            product.AddedOn = addedOn;
            product.CategoryId = productModel.CategoryId;

            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var productDetails = await dbContext.Products.Include(p => p.Category).Where(p => p.IsDeleted == false && p.Id == id)
                .Select(p => new DetailsProductViewModel()
                {
                    Description = p.Description,
                    AddedOn = p.AddedOn.ToString(ComanConsts.DateFormat),
                    CategoryName = p.Category.Name,
                    Id = p.Id,
                    ImageUrl = p.ImageUrl, 
                    Price = p.Price,
                    ProductName= p.ProductName,
                    Seller = p.Seller.UserName ?? string.Empty,
                    HasBought = p.ProductsClients.Any(pc => pc.ClientId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? true : false
                }).AsNoTracking().FirstOrDefaultAsync();
            if (productDetails == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(productDetails);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await dbContext.Products.Where(p => p.Id == id).AsNoTracking().FirstOrDefaultAsync();
            
            if (product == null || product.IsDeleted)
            {
                return RedirectToAction(nameof(Index));
            }


            await dbContext.ProductClients.AddAsync(new ProductClient()
            {
                ProductId = product.Id,
                ClientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var product = await dbContext.Products.Include(p => p.ProductsClients).Where(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null || product.IsDeleted)
            {
                return RedirectToAction(nameof(Index));
            }

            var productClient = product.ProductsClients
                .FirstOrDefault(pc => pc.ClientId == (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty));
            
            if (productClient != null)
            {
                product.ProductsClients.Remove(productClient);
                await dbContext.SaveChangesAsync();
            }  

            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            
                var deleteProduct = await dbContext.Products.Where(p => p.Id == id && p.IsDeleted == false).Select(p => new DeleteProductViewModel() 
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Seller = p.Seller,
                SellerId = p.SellerId

            }).AsNoTracking().FirstOrDefaultAsync();

            if (deleteProduct == null || deleteProduct.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction(nameof(Index));
            }
            

            return View(deleteProduct);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteProductViewModel deleteProductModel)
        {
            
            if (deleteProductModel.SellerId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                var deleteProduct = await dbContext.Products.Where(p => p.Id == deleteProductModel.Id && p.IsDeleted == false).FirstOrDefaultAsync();
                if (deleteProduct != null)
                {
                    deleteProduct.IsDeleted = true;
                    await dbContext.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }


    }
}

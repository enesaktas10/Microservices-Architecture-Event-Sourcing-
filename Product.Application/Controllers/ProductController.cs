using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Product.Application.Models.ViewModels;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.Application.Controllers
{
    public class ProductController(IEventStoreService eventStoreService, IMongoDBService mongoDBService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var products = await (await productCollection.FindAsync(_ => true)).ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM model)
        {
            NewProductAddedEvent newProductAddedEvent = new()
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = model.ProductName,
                InitialCount = model.Count,
                InitialPrice = model.Price,
                IsAvailable = model.IsAvaliable
            };


            await eventStoreService.AppendToStreamAsync("products-stream", new[] { eventStoreService.GenerateEventData(newProductAddedEvent) });


            //URUN EKLENDIKTEN SONRA TEKRARDAN LISTETE DONMEK ICIN INDEX E REDIRECT ETTIM
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string productId)
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == productId)).FirstOrDefaultAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> CountUpdate(Shared.Models.Product model,int durum)
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (durum ==1)
            {
                CountDecreasedEvent countDecreasedEvent = new()
                {
                    ProductId = model.Id,
                    DecrementAmount = model.Count
                };

                await eventStoreService.AppendToStreamAsync("products-stream", new[] {eventStoreService.GenerateEventData(countDecreasedEvent) });

            }
            else if(durum==0)
            {
                CountIncreasedEvent countIncreasedEvent = new()
                {
                    IncrementAmount = model.Count,
                    ProductId = model.Id
                };

                await eventStoreService.AppendToStreamAsync("products-stream", new[] { eventStoreService.GenerateEventData(countIncreasedEvent) });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PriceUpdate(Shared.Models.Product model,int durum)
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (durum==1)
            {
                PriceDecreasedEvent priceDecreasedEvent = new()
                {
                    DecrementAmount = model.Price,
                    ProductId = model.Id
                };

                await eventStoreService.AppendToStreamAsync("products-stream", new[] { eventStoreService.GenerateEventData(priceDecreasedEvent) });
            }
            else if (durum==0)
            {
                PriceIncreasedEvent priceIncreasedEvent = new()
                {
                    ProductId = model.Id,
                    IncrementAmount = model.Price
                };

                await eventStoreService.AppendToStreamAsync("products-stream", new[] { eventStoreService.GenerateEventData(priceIncreasedEvent) });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AvailableUpdate(Shared.Models.Product model)
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (product.IsAvailable != model.IsAvailable)
            {
                AvailablilityChangedEvent availablilityChangedEvent = new()
                {
                    IsAvailable = model.IsAvailable,
                    ProductId = model.Id
                };

                await eventStoreService.AppendToStreamAsync("products-stream", new[] { eventStoreService.GenerateEventData(availablilityChangedEvent) });
            }
            
            return RedirectToAction("Index");
        }

    }
}

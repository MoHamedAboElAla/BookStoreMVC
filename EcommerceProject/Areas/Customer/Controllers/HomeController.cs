using Microsoft.AspNetCore.Mvc;
using Ecommerce.Models;
using System.Diagnostics;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Ecommerce.DataAcess.Data;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Http;

namespace EcommerceProject.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, AppDbContext context)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
         
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims!=null)
            {
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(
                 u => u.ApplicationUserId == claims.Value).Count());

            }


            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includePropperties: "Category");
            return View(productList);
        }
        public IActionResult Details(int id)
        {
            ShoppingCart cart = new() {
                Product = _unitOfWork.Product.Get(i => i.Id == id, includePropperties: "Category"),
                Count = 1,
                ProductId=id
            };

            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb=_unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                TempData["success"] = "Cart Updated Successfully";
                _unitOfWork.Save();

            }
            else
            {
                  shoppingCart.Id = 0;
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId).Count());
                TempData["success"] = "Cart Added Successfully";


            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

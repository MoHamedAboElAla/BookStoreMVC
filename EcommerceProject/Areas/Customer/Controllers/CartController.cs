
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace EcommerceProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]

    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCart = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
                , includePropperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in shoppingCart.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCart.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(shoppingCart);
        }
        public IActionResult Summary()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCart = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
                , includePropperties: "Product"),
                OrderHeader = new()
            };

            shoppingCart.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(i=>i.Id==userId);
            
            shoppingCart.OrderHeader.Name = shoppingCart.OrderHeader.ApplicationUser.Name;
            shoppingCart.OrderHeader.Phone = shoppingCart.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCart.OrderHeader.StreetAddress = shoppingCart.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCart.OrderHeader.City = shoppingCart.OrderHeader.ApplicationUser.City;
            shoppingCart.OrderHeader.State = shoppingCart.OrderHeader.ApplicationUser.State;
            shoppingCart.OrderHeader.PostalCode = shoppingCart.OrderHeader.ApplicationUser.PostalCode;




            foreach (var cart in shoppingCart.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCart.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(shoppingCart);
        }


        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM.ListCart=_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
				includePropperties: "Product");

			ApplicationUser applicationUser= _unitOfWork.ApplicationUser.Get(i => i.Id == userId);

			
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;



			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Regular customer account 
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
				ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
			}
			else 
            {
				// Company user account
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
				ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
			}
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();
                
            foreach(var cart in ShoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                // Regular customer account and we need to capture payment
                //Stripe logic
                StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

                var domain = "https://localhost:7097/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl  =domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                       Mode = "payment",
                };

                foreach(var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData=new SessionLineItemPriceDataOptions
                        {

                            UnitAmount=(long)(item.Price * 100),
                            Currency = "EGP",
                            ProductData=new SessionLineItemPriceDataProductDataOptions 
                            {
                            Name=item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    }; 
                    options.LineItems.Add(sessionLineItem);
                }
                  var service = new Stripe.Checkout.SessionService();
                  Session session = service.Create(options);
                 _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                 _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);



            }

            return RedirectToAction(nameof(OrderConfirmation),new {id=ShoppingCartVM.OrderHeader.Id});
		}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader=_unitOfWork.OrderHeader.Get(i=>i.Id==id,includePropperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
            {
                //order by customer
                var service=new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                   _unitOfWork.OrderHeader.UpdateStripePaymentId(id,session.Id,session.PaymentIntentId);
                   _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.StatusApproved,StaticDetails.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }

           List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(i => i.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(i => i.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(i => i.Id == cartId,tracked:true);
            if (cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(
             u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(i => i.Id == cartId,tracked:true);
            HttpContext.Session.SetInt32(StaticDetails.SessionCart, _unitOfWork.ShoppingCart.GetAll(
               u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);           
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }




        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;

                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }

        }

    }
}

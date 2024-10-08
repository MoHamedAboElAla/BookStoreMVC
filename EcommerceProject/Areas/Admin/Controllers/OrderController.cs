using Ecommerce.DataAcess.Repository;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace EcommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {

        [BindProperty]
        public OrderVM orderVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(i => i.Id == orderId, includePropperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(i => i.OrderHeaderId == orderId, includePropperties: "Product")
            };

            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var HeaderFromDb = _unitOfWork.OrderHeader.Get(i => i.Id == orderVM.OrderHeader.Id);
            HeaderFromDb.Name = orderVM.OrderHeader.Name;
            HeaderFromDb.Phone = orderVM.OrderHeader.Phone;
            HeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            HeaderFromDb.City = orderVM.OrderHeader.City;
            HeaderFromDb.State = orderVM.OrderHeader.State;
            HeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                HeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                HeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(HeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = HeaderFromDb.Id });
        }


        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult StartProcessing() {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, StaticDetails.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = _unitOfWork.OrderHeader.Get(i => i.Id == orderVM.OrderHeader.Id);
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = System.DateTime.Now;
            orderHeader.OrderStatus = StaticDetails.StatusShipped;

            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(i => i.Id == orderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId

                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.StatusCancelled);

            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully";
            return RedirectToAction(nameof(Index));

        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            orderVM.OrderHeader = _unitOfWork.OrderHeader.Get(i => i.Id == orderVM.OrderHeader.Id, includePropperties: "ApplicationUser");
            orderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(i => i.OrderHeaderId == orderVM.OrderHeader.Id, includePropperties: "Product");
            //Stripe logic
            // StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";

            var domain = "https://localhost:7097/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {

                        UnitAmount = (long)(item.Price * 100),
                        Currency = "EGP",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        

        }


        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(i => i.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
            {
                //order by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,orderHeader.OrderStatus, StaticDetails.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        #region API CALLS


        [HttpGet]
        public IActionResult GetAll(string status)
        {
       
            IEnumerable<OrderHeader> OrderList;

            if(User.IsInRole(StaticDetails.Role_Admin)|| User.IsInRole(StaticDetails.Role_Employee))
            {
                OrderList = _unitOfWork.OrderHeader.GetAll(includePropperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                OrderList = _unitOfWork.OrderHeader.GetAll(i => i.ApplicationUserId == userId,includePropperties: "ApplicationUser");
            }
            switch (status)
            {
                case "pending":
                    OrderList = OrderList.Where(u=>u.PaymentStatus==StaticDetails.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    OrderList = OrderList.Where(u => u.OrderStatus == StaticDetails.StatusInProcess);
                    break;


                case "completed":
                    OrderList = OrderList.Where(u => u.OrderStatus == StaticDetails.StatusShipped);
                    break;


                case "approved":
                    OrderList = OrderList.Where(u => u.OrderStatus == StaticDetails.StatusApproved);
                    break;

                default:
                    break;


            }
            return Json(new { data = OrderList });
        }

       
        #endregion

    }
}

﻿namespace CourseSales.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public OrderController(
            IBasketService basketService, 
            IOrderService orderService)
        {
            _basketService = basketService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            ViewBag.Basket = await _basketService.GetAsync();

            return View(new CheckoutInfoInput());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutInfoInput checkoutInfoInput)
        {
            //1. yol senkron iletişim
            //var orderStatus = await _orderService.CreateOrderAsync(checkoutInfoInput);
            // 2.yol asenkron iletişim
            var orderSuspend = await _orderService.SuspendOrderAsync(checkoutInfoInput);
            if (!orderSuspend.IsSuccessful)
            {
                var basket = await _basketService.GetAsync();

                ViewBag.Basket = basket;
                ViewBag.Error = orderSuspend.Error;

                return View();
            }
            //1. yol senkron iletişim
            //  return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = orderStatus.OrderId });

            //2.yol asenkron iletişim
            return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = new Random().Next(1, 1000) });
        }

        public IActionResult SuccessfulCheckout(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public async Task<IActionResult> CheckoutHistory()
        {
            var orders = await _orderService.GetOrderAsync();
            return View(orders);
        }
    }
}

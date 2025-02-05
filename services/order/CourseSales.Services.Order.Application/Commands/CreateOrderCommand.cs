﻿namespace CourseSales.Services.Order.Application.Commands
{
    public class CreateOrderCommand : IRequest<Shared.DataTransferObjects.Response<CreatedOrderDto>>
    {
        public string BuyerId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public AddressDto Address { get; set; }
    }
}

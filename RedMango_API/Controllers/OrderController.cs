using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Service;
using RedMango_API.Models.Dto;
using RedMango_API.Utility;

namespace RedMango_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext _db;
   
    private ApiResponse _response;

    public OrderController(ApplicationDbContext db)
    {
        _db = db;
      
        _response= new ApiResponse();
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
    {
        try
        {
            var OrderHeaders = _db.OrderHeaders.Include(u => u.OrderDetails).
                ThenInclude(u => u.MenuItem).OrderByDescending(u => u.OrderHeaderId);
            if(!string.IsNullOrEmpty(userId))
            {
                _response.Result = OrderHeaders.Where(u => u.ApplicationUserId == userId);
            }
            else
            {
                _response.Result = OrderHeaders;
            }

            return Ok(_response);

        }
        catch (Exception ex)
        {
            _response.IsSuccess=false;
            _response.ErrorMessages=new List<string>()
            { 
                ex.ToString()
            };
            
        }
        return _response;
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse>> GetOrders(int id )
    {
        try
        {
           
            if (id==0)
            {
                _response.StatusCode=HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
             var OrderHeaders=_db.OrderHeaders.Include(u=>u.OrderDetails).ThenInclude(u=>u.MenuItem)
                .Where(u=>u.OrderHeaderId==id);
               
            if(OrderHeaders==null)
            {
                _response.StatusCode =HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = OrderHeaders;
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
            
        }

        
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };

        }
        return _response;
    }
    [HttpPost]
    public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
    {
        try
        {
            OrderHeader order = new()
            { 
                ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                PickupEmail = orderHeaderDTO.PickupEmail,
                PickupName = orderHeaderDTO.PickupName,
                PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                OrderTotal = orderHeaderDTO.OrderTotal,
                OrderDate =DateTime.Now,
                StripePaymentIntentID=orderHeaderDTO.StripePaymentIntentID,
                TotalItems = orderHeaderDTO.TotalItems,
                Status = String.IsNullOrEmpty(orderHeaderDTO.Status)?SD.status_pending : orderHeaderDTO.Status,
                
            };
            if (ModelState.IsValid) 
            {
                _db.OrderHeaders.Add(order);
                _db.SaveChanges();
                foreach (var orderDetailsDTO in orderHeaderDTO.OrderDetailsDTO)
                {
                    OrderDetails orderDetails = new()
                    {
                        OrderHeaderId=order.OrderHeaderId,
                        ItemName=orderDetailsDTO.ItemName,
                        MenuItemId=orderDetailsDTO.MenuItemId,
                        Price=orderDetailsDTO.Price,
                        Quantity=orderDetailsDTO.Quantity,


                    };
                    _db.OrderDetails.Add(orderDetails);
                }
                _db.SaveChanges();
                _response.Result = order;
                order.OrderDetails = null;
                _response.StatusCode=HttpStatusCode.Created;
                return Ok(_response);
            }

        }
        catch (Exception ex)
        {

            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };
        }
        return _response;

    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDTO orderHeaderDTO)
    {
        try
        {
            if (orderHeaderDTO == null || id !=orderHeaderDTO.OrderHeaderId)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            OrderHeader orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.OrderHeaderId == id);    

            if (orderFromDb == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if (!string.IsNullOrEmpty(orderHeaderDTO.PickupName))
            {
                orderFromDb.PickupName = orderHeaderDTO.PickupName;
            }
            if (!string.IsNullOrEmpty(orderHeaderDTO.PickupEmail))
            {
                orderFromDb.PickupEmail = orderHeaderDTO.PickupEmail;
            }
            if (!string.IsNullOrEmpty(orderHeaderDTO.PickupPhoneNumber))
            {
                orderFromDb.PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber;
            }
            if (!string.IsNullOrEmpty(orderHeaderDTO.StripePaymentIntentID))
            {
                orderFromDb.StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID;
            }
            if (!string.IsNullOrEmpty(orderHeaderDTO.Status))
            {
                orderFromDb.Status = orderHeaderDTO.Status;
            }
            _db.SaveChanges();
            _response.StatusCode=HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);

        }
        catch (Exception ex)
        {

            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>()
            {
                ex.ToString()
            };
        }
        return _response;   
    }

}


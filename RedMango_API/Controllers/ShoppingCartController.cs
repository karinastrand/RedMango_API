using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using System.Net;


namespace RedMango_API.Controllers;

[Route("api/shoppingCart")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    protected ApiResponse  _response;
    private readonly ApplicationDbContext _db;

    public ShoppingCartController(ApplicationDbContext db)
    {
        _db = db;
        _response = new ApiResponse();
        
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
    {
        try
        {
            ShoppingCart shoppingCart;
            if (string.IsNullOrEmpty(userId))
            {
                shoppingCart = new ShoppingCart();
             //   _response.IsSuccess = false;
            //   _response.StatusCode = HttpStatusCode.BadRequest;
              //  return BadRequest(_response);
            }
            else
            {
                shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).
               ThenInclude(u => u.MenuItem).FirstOrDefault(u => u.UserId == userId);
            }
            
            if(shoppingCart.CartItems!=null && shoppingCart.CartItems.Count>0)
            {
                shoppingCart.CartTotal=shoppingCart.CartItems.Sum(u=>u.Quantity*u.MenuItem.Price);
            }
           

            _response.Result=shoppingCart;
            _response.IsSuccess=true;
            _response.StatusCode=HttpStatusCode.OK;
            return _response;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages=
                new List<string>() { ex.ToString()};
            _response.StatusCode = HttpStatusCode.BadRequest;
            return _response;
        }

    }



    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
    {

        ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u=>u.CartItems).FirstOrDefault(u => u.UserId == userId);
        MenuItem menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == menuItemId);
        if (menuItemId == 0)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            return BadRequest(_response);
        }
        if (shoppingCart == null && updateQuantityBy > 0)
        {
            //create a shopping cart and add cart item
            ShoppingCart newCart = new()
            {
                UserId = userId
            };
            _db.ShoppingCarts.Add(newCart);
            _db.SaveChanges();
            CartItem newCartItem = new()
            {
                MenuItemId = menuItemId,
                Quantity = updateQuantityBy,
                ShoppingCartId = newCart.Id,
                MenuItem = null


            };
            _db.CartItems.Add(newCartItem);
            _db.SaveChanges();
        }
        else
        {
            //shopping cart exists
            CartItem cartItemInCart=shoppingCart.CartItems.FirstOrDefault(u => u.MenuItemId == menuItemId);
            if (cartItemInCart == null)
            {
                //item does not exist in the current cart

                CartItem newCartItem = new() 
                {
                    MenuItemId=menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId=shoppingCart.Id,
                    MenuItem = null
                };
                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();
            }
            else
            {
                //item already exist in the cart and we have to update quantity
                int newQuantity=cartItemInCart.Quantity+updateQuantityBy;

                if (updateQuantityBy == 0 || newQuantity<=0)
                {
                    //remove cart item freom cart and if it is the only item then remove cart
                    _db.CartItems.Remove(cartItemInCart);
                    if (shoppingCart.CartItems.Count == 1)
                    {
                        _db.ShoppingCarts.Remove(shoppingCart);
                    }
                    _db.SaveChanges();
                }
                else
                {
                    cartItemInCart.Quantity = newQuantity;
                    _db.SaveChanges();
                }
            }



        }

        return _response;
    }


}

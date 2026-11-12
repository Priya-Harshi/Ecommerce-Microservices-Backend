using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // Customer/Admin can create an order
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderDto dto)
    {
        try
        {
            var order =
                await _orderService
                    .CreateOrderAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Customer can view their own order.
    // Admin can view any order.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order =
            await _orderService.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        // Admin can view any order
        if (User.IsInRole("Admin"))
        {
            return Ok(order);
        }

        // Get logged-in customer's ID from JWT
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId =
            int.Parse(userIdClaim.Value);

        // Customer can only view their own order
        if (order.UserId != userId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        if (!User.IsInRole("Admin"))
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var loggedInUserId =
                int.Parse(userIdClaim.Value);

            if (loggedInUserId != userId)
            {
                return Forbid();
            }
        }

        var orders =
            await _orderService.GetByUserIdAsync(userId);

        return Ok(orders);
    }

    // Only Customer can checkout
    [Authorize(Roles = "Customer")]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        CheckoutRequest request)
    {
        var result =
            await _orderService.CheckoutAsync(
                request.OrderId);

        if (!result)
        {
            return BadRequest(
                "Checkout failed. Order may not exist or is not pending.");
        }

        return Ok(
            "Checkout and payment successful. Order is now paid.");
    }
}
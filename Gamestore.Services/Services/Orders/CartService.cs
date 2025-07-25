﻿using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Orders;

/// <summary>
/// Service for managing shopping cart operations including adding games, 
/// updating quantities, and processing cart contents.
/// </summary>
public class CartService(IUnitOfWork unitOfWork, ILogger<CartService> logger) : ICartService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CartService> _logger = logger;

    /// <summary>
    /// Adds a game to the customer's shopping cart with specified quantity.
    /// </summary>
    /// <param name="gameKey">Unique identifier of the game to add</param>
    /// <param name="customerId">Customer's unique identifier</param>
    /// <param name="quantity">Number of games to add (default: 1)</param>
    public async Task AddGameToCartAsync(string gameKey, Guid customerId, int quantity = 1)
    {
        _logger.LogInformation("Adding game {GameKey} to cart for customer {CustomerId} with quantity {Quantity}",
            gameKey, customerId, quantity);

        var game = await _unitOfWork.Games.GetKeyAsync(gameKey) ?? throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        if (game.UnitInStock < quantity)
        {
            throw new ValidationException($"Insufficient stock. Available: {game.UnitInStock}, Requested: {quantity}");
        }

        var cart = await GetOrCreateCartAsync(customerId);

        var existingOrderGame = await _unitOfWork.OrderGames.GetOrderGameAsync(cart.Id, game.Id);

        if (existingOrderGame != null)
        {
            var newQuantity = existingOrderGame.Quantity + quantity;

            if (newQuantity > game.UnitInStock)
            {
                throw new ValidationException($"Total quantity ({newQuantity}) exceeds available stock ({game.UnitInStock})");
            }

            await _unitOfWork.OrderGames.UpdateQuantityAsync(existingOrderGame.Id, newQuantity);
            _logger.LogInformation("Updated quantity for game {GameKey} in cart to {NewQuantity}", gameKey, newQuantity);
        }
        else
        {
            var orderGame = new OrderGame
            {
                OrderId = cart.Id,
                ProductId = game.Id,
                Price = game.Price,
                Quantity = quantity,
                Discount = game.Discontinued
            };

            await _unitOfWork.OrderGames.AddAsync(orderGame);
            _logger.LogInformation("Added new game {GameKey} to cart with quantity {Quantity}", gameKey, quantity);
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task RemoveGameFromCartAsync(string gameKey, Guid customerId)
    {
        _logger.LogInformation("Removing game {GameKey} from cart for customer {CustomerId}", gameKey, customerId);

        var game = await _unitOfWork.Games.GetKeyAsync(gameKey) ?? throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart == null)
        {
            return;
        }

        await _unitOfWork.OrderGames.RemoveOrderGameAsync(cart.Id, game.Id);

        var remainingItems = await _unitOfWork.OrderGames.GetOrderGamesByOrderIdAsync(cart.Id);
        if (!remainingItems.Any())
        {
            await _unitOfWork.Orders.DeleteAsync(cart.Id);
            _logger.LogInformation("Deleted empty cart for customer {CustomerId}", customerId);
        }

        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Removed game {GameKey} from cart for customer {CustomerId}", gameKey, customerId);
    }

    public async Task<IEnumerable<OrderGameDto>> GetCartAsync(Guid customerId)
    {
        _logger.LogInformation("Getting cart for customer {CustomerId}", customerId);

        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        return cart == null
            ? Enumerable.Empty<OrderGameDto>()
            : cart.OrderGames.Select(og => new OrderGameDto
            {
                ProductId = og.ProductId,
                Price = og.Price,
                Quantity = og.Quantity,
                Discount = og.Discount,
                TotalPrice = og.TotalPrice,
                GameName = og.Product?.Name,
                GameKey = og.Product?.Key
            });
    }

    public async Task ClearCartAsync(Guid customerId)
    {
        _logger.LogInformation("Clearing cart for customer {CustomerId}", customerId);

        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart != null)
        {
            await _unitOfWork.Orders.DeleteAsync(cart.Id);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task UpdateCartItemQuantityAsync(string gameKey, Guid customerId, int newQuantity)
    {
        _logger.LogInformation("Updating cart item quantity for game {GameKey}, customer {CustomerId} to {NewQuantity}",
            gameKey, customerId, newQuantity);

        var game = await _unitOfWork.Games.GetKeyAsync(gameKey) ?? throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId) ?? throw new KeyNotFoundException("Cart not found");
        var orderGame = await _unitOfWork.OrderGames.GetOrderGameAsync(cart.Id, game.Id) ?? throw new KeyNotFoundException($"Game '{gameKey}' not found in cart");
        if (newQuantity <= 0)
        {
            await RemoveGameFromCartAsync(gameKey, customerId);
            return;
        }

        if (newQuantity > game.UnitInStock)
        {
            throw new ValidationException($"Requested quantity ({newQuantity}) exceeds available stock ({game.UnitInStock})");
        }

        await _unitOfWork.OrderGames.UpdateQuantityAsync(orderGame.Id, newQuantity);
        await _unitOfWork.CompleteAsync();
    }

    private async Task<Order> GetOrCreateCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart != null)
        {
            return cart;
        }

        var newCart = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Open
        };

        await _unitOfWork.Orders.AddAsync(newCart);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Created new cart for customer {CustomerId}", customerId);
        return newCart;
    }
}


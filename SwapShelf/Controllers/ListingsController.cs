using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using System.Security.Claims;

namespace SwapShelf.Controllers
{
    [ApiController]
    [Route("api/listings")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var listings = await _listingService.GetByUserAsync(GetUserId());
            return Ok(listings);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? genre,
            [FromQuery] string? condition,
            [FromQuery] string? location,
            [FromQuery] string? author)
        {
            var listings = await _listingService.GetAllAsync(genre, condition, location, author);
            return Ok(listings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var listing = await _listingService.GetByIdAsync(id);
                return Ok(listing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ListingRequest request)
        {
            try
            {
                var listing = await _listingService.CreateAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetById), new { id = listing.Id }, listing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ListingRequest request)
        {
            try
            {
                var listing = await _listingService.UpdateAsync(GetUserId(), id, request);
                return Ok(listing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _listingService.DeleteAsync(GetUserId(), id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
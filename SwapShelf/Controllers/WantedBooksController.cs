using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using System.Security.Claims;

namespace SwapShelf.Controllers
{
    [ApiController]
    [Route("api/wanted")]
    public class WantedBooksController : ControllerBase
    {
        private readonly IWantedBookService _wantedBookService;

        public WantedBooksController(IWantedBookService wantedBookService)
        {
            _wantedBookService = wantedBookService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyWanted()
        {
            var wanted = await _wantedBookService.GetByUserAsync(GetUserId());
            return Ok(wanted);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] WantedBookRequest request)
        {
            try
            {
                var wanted = await _wantedBookService.AddAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetMyWanted), wanted);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                await _wantedBookService.RemoveAsync(GetUserId(), id);
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
        }
    }
}
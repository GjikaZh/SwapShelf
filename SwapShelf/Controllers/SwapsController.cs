using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using System.Security.Claims;

namespace SwapShelf.Controllers
{
    [ApiController]
    [Route("api/swaps")]
    [Authorize]
    public class SwapsController : ControllerBase
    {
        private readonly ISwapService _swapService;

        public SwapsController(ISwapService swapService)
        {
            _swapService = swapService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetMySwaps()
        {
            var swaps = await _swapService.GetByUserAsync(GetUserId());
            return Ok(swaps);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var swap = await _swapService.GetByIdAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SwapRequestCreate request)
        {
            try
            {
                var swap = await _swapService.CreateAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetById), new { id = swap.Id }, swap);
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

        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            try
            {
                var swap = await _swapService.AcceptAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var swap = await _swapService.RejectAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/transit")]
        public async Task<IActionResult> MarkInTransit(int id)
        {
            try
            {
                var swap = await _swapService.MarkInTransitAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var swap = await _swapService.CompleteAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var swap = await _swapService.CancelAsync(id, GetUserId());
                return Ok(swap);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
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
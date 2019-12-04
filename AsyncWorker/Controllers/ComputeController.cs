using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AsyncWorker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputeController : ControllerBase
    {
        private readonly IStorage<ComputationRequest> _requests;
        private readonly IStorage<ComputationResult> _results;
        private readonly IQueue _queue;

        public ComputeController(
            IStorage<ComputationRequest> requests,
            IStorage<ComputationResult> results,
            IQueue queue)
        {
            _requests = requests;
            _results = results;
            _queue = queue;
        }

        [HttpGet("status")]
        public async Task<ActionResult<object>> GetStatusAsync()
        {
            var result = await _queue.GetCountAsync(HttpContext.RequestAborted);

            return new
            {
                Count = result
            };
        }

        [HttpGet("{id}", Name = nameof(GetResultAsync))]
        public async Task<ActionResult<ComputationResult>> GetResultAsync(string id)
        {
            var result = await _results.GetAsync(id, HttpContext.RequestAborted);

            if (result is null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> Post(ComputationRequest request)
        {
            var id = Guid.NewGuid().ToString();

            await _requests.SaveAsync(id, request, HttpContext.RequestAborted);
            await _queue.EnqueueAsync(id, HttpContext.RequestAborted);

            return AcceptedAtRoute(nameof(GetResultAsync), new { Id = id });
        }
    }
}

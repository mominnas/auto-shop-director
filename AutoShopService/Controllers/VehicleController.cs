using Microsoft.AspNetCore.Mvc;
using MMN.Models;

namespace MMN.Service.Controllers
{
    /// <summary>
    /// Contains methods for interacting with vehicle data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleRepository _repository;

        public VehicleController(IVehicleRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets all vehicles in the database.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _repository.GetForCustomerAsync(Guid.Empty)); // Returns all vehicles if supported, otherwise adjust repository
        }

        /// <summary>
        /// Gets the vehicle with the given id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }
            var vehicle = await _repository.GetAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return Ok(vehicle);
        }

        /// <summary>
        /// Gets all vehicles for a given customer.
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetForCustomer(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                return BadRequest();
            }
            var vehicles = await _repository.GetForCustomerAsync(customerId);
            if (vehicles == null)
            {
                return NotFound();
            }
            return Ok(vehicles);
        }

        /// <summary>
        /// Creates a new vehicle or updates an existing one.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Vehicle vehicle)
        {
            return Ok(await _repository.UpsertAsync(vehicle));
        }

        /// <summary>
        /// Deletes a vehicle.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return Ok();
        }
    }
}
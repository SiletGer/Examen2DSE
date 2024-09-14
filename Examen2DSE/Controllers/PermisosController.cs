using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Examen2DSE.Data;
using Examen2DSE.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examen2DSE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly string cacheKeyPermisos = "permisos"; 

        public PermisosController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/Permisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermisos()
        {
            var cachedPermisos = await _cache.GetStringAsync(cacheKeyPermisos);
            if (cachedPermisos != null)
            {
                return Ok(JsonSerializer.Deserialize<IEnumerable<Permiso>>(cachedPermisos));
            }
            var permisos = await _context.Permisos.ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _cache.SetStringAsync(cacheKeyPermisos, JsonSerializer.Serialize(permisos), cacheOptions);

            return Ok(permisos);
        }

        // GET: api/Permisos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> GetPermiso(int id)
        {
            var cacheKey = $"permiso_{id}";
            var cachedPermiso = await _cache.GetStringAsync(cacheKey);
            if (cachedPermiso != null)
            {
                return Ok(JsonSerializer.Deserialize<Permiso>(cachedPermiso));
            }

            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(permiso), cacheOptions);

            return Ok(permiso);
        }

        // POST: api/Permisos
        [HttpPost]
        public async Task<ActionResult<Permiso>> PostPermiso(Permiso permiso)
        {
            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(cacheKeyPermisos);

            return CreatedAtAction(nameof(GetPermiso), new { id = permiso.Id }, permiso);
        }

        // PUT: api/Permisos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermiso(int id, Permiso permiso)
        {
            if (id != permiso.Id)
            {
                return BadRequest();
            }

            _context.Entry(permiso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            await _cache.RemoveAsync(cacheKeyPermisos);
            await _cache.RemoveAsync($"permiso_{id}");

            return NoContent();
        }

        // DELETE: api/Permisos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermiso(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(cacheKeyPermisos);
            await _cache.RemoveAsync($"permiso_{id}");

            return NoContent();
        }

        private bool PermisoExists(int id)
        {
            return _context.Permisos.Any(e => e.Id == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DN6SimpleWebWithAuth.Data;
using DN6SimpleWebWithAuth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;

namespace DN6SimpleWebWithAuth.Controllers
{
    //some code from: https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats/Controllers/HomeController.cs
    public class StatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Task<RedisConnection> _redisConnectionFactory;
        private RedisConnection _redisConnection;
        private readonly IConfiguration _configuration;

        private const string STATES_KEY = "States";

        public StatesController(ApplicationDbContext context, IConfiguration configuration, Task<RedisConnection> redisConnectionFactory)
        {
            _context = context;
            _configuration = configuration;
            _redisConnectionFactory = redisConnectionFactory;
        }

        private async Task<List<State>> GetStates()
        {
            return await _context.States.OrderBy(x => x.Name).ToListAsync();
        }

        private async Task<List<State>> AddOrUpdateStatesInCache()
        {
            var states = await GetStates();
            var statesJSON = JsonSerializer.Serialize(states);

            //add to cache
            _redisConnection = await _redisConnectionFactory;
            await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(STATES_KEY, statesJSON));

            return states;
        }

        private async Task<List<State>> GetStatesFromCache()
        {
            _redisConnection = await _redisConnectionFactory;
            var result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(STATES_KEY))).ToString();
            
            if (string.IsNullOrWhiteSpace(result))
            {
                return await AddOrUpdateStatesInCache();
            }
                
            var data = JsonSerializer.Deserialize<List<State>>(result);
            return data;
        }

        private async Task InvalidateStates()
        {
            _redisConnection = await _redisConnectionFactory;
            await _redisConnection.BasicRetryAsync(async (db) => await db.KeyDeleteAsync(STATES_KEY));
        }

        // GET: States
        public async Task<IActionResult> Index()
        {
            var states = await GetStatesFromCache();

            return states != null && states.Any() ?
                        View(states) :
                        Problem("Entity set and/or cache for 'ApplicationDbContext.States'  is null.");
        }

        // GET: States/Details/5
        public async Task<IActionResult> Details(int? id)
        { 
            if (id == null || _context.States == null)
            {
                return NotFound();
            }

            var states = await GetStatesFromCache();
            var state = states.FirstOrDefault(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // GET: States/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: States/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Abbreviation")] State state)
        {
            if (ModelState.IsValid)
            {
                _context.Add(state);
                await _context.SaveChangesAsync();
                await InvalidateStates();
                return RedirectToAction(nameof(Index));
            }
            return View(state);
        }

        // GET: States/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.States == null)
            {
                return NotFound();
            }

            var states = await GetStatesFromCache();
            var state = states.FirstOrDefault(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }
            return View(state);
        }

        // POST: States/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Abbreviation")] State state)
        {
            if (id != state.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var stateToUpdate = await _context.States.FirstOrDefaultAsync(m => m.Id == id);
                    stateToUpdate.Abbreviation = state.Abbreviation;
                    stateToUpdate.Name = state.Name;
                    _context.Update(stateToUpdate);
                    await _context.SaveChangesAsync();
                    await InvalidateStates();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StateExists(state.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(state);
        }

        // GET: States/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.States == null)
            {
                return NotFound();
            }

            var states = await GetStatesFromCache();
            var state = states.FirstOrDefault(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // POST: States/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.States == null)
            {
                return Problem("Entity set 'ApplicationDbContext.States'  is null.");
            }
            var state = await _context.States.FindAsync(id);
            if (state != null)
            {
                _context.States.Remove(state);
            }
            
            await _context.SaveChangesAsync();
            await InvalidateStates();
            return RedirectToAction(nameof(Index));
        }

        private bool StateExists(int id)
        {
          return (_context.States?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

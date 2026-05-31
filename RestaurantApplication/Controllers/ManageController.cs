using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantApplication.Data;
using RestaurantApplication.Models;
using System.Data;

namespace RestaurantApplication.Controllers
{
    public class ManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageController(ApplicationDbContext context) 
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var sittings = _context.Sittings
                .Include(s=>s.Reservations)
                .Include(s => s.SittingType)
                .ToList();
            return View(sittings);
        }

        //Sitting Type Manage
        //Sitting Type - Create
        [HttpGet]
        public IActionResult SittingTypeCreate()
        {
            return View();
        }
        [HttpPost]
        [ActionName(nameof(SittingTypeCreate))]
        public async Task<IActionResult> SittingTypeCreateConfirm(SittingType t)
        {
            if (ModelState.IsValid)
            {
                var s = new SittingType()
                {
                    Name = t.Name
                };
                _context.SittingTypes.Add(s);
                await _context.SaveChangesAsync();
                return RedirectToAction("List");
            }
            return View(t);
        }
        //Sitting Type list
        public IActionResult SittingTypeList()
        {
            var sittingTypes = _context.SittingTypes.ToList();
            return View(sittingTypes);
        }
        //Sitting Type Delete
        [HttpGet]
        public async Task<IActionResult> SittingTypeDelete(int id) 
        {
            var sittingType = await _context.SittingTypes
                .FirstOrDefaultAsync(s => s.Id == id);
            if(sittingType == null)
            {
                return NotFound();
            }
            return View(sittingType);
        }
        [HttpPost]
        [ActionName(nameof(SittingTypeDelete))]
        public async Task<IActionResult> SittingTypeDeleteConfirm(int id)
        {
            var sittingTypes = await _context.SittingTypes
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sittingTypes == null)
            {
                return NotFound();
            }
            _context.SittingTypes.Remove(sittingTypes);

            await _context.SaveChangesAsync();
            return RedirectToAction("List");
        }



        public async Task<IActionResult> Details(int id)
        {
            var sittings = await _context.Sittings
                .Include(s=>s.Reservations)

                .Include(s => s.SittingType)
                .FirstOrDefaultAsync(s => s.Id == id);
            if(sittings == null)
            {
                return NotFound();
            }
            return View(sittings);
        }

        //Sittings - Create
        public IActionResult SittingsCreate()
        {
            var sittings = _context.Sittings
                .Include(s => s.SittingType)
                .ToList();
            var sittingTypes = _context.SittingTypes.ToList();
            var sets = new SetSittingVM()
            {
                SittingTypes = sittingTypes,
            };

            return View(sets);
        }

        [HttpPost]
        public async Task<IActionResult> SittingsCreate(SetSittingVM s)
        {
            var st = await _context.SittingTypes
                .FirstOrDefaultAsync(t => t.Name == s.selectedSittingType);
            if (st == null)
            {
                st = new SittingType { Name = s.selectedSittingType };
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(r => r.Id == s.restaurantId);
            if(restaurant == null)
            {
                restaurant = new Restaurant { Id = s.restaurantId };
            }

            st.Name = s.selectedSittingType;

            var sitting = new Sitting
            {
                Capacity = s.capacity,
                StartTime = s.startTime,
                EndTime = s.endTime,
                Active = s.active,
                RestaurantId = s.restaurantId = 1,
                SittingType = st
            };

            _context.Sittings.Add(sitting);

            int repeatForNumberOfDays = s.numberOfRepeat;
            var start = s.startTime;
            var end = s.endTime;
            var sittingType = st;
            var active = s.active;
            var capacity = s.capacity;

            for(int i = 0; i < repeatForNumberOfDays; i++)
            {
                start = start.AddDays(1);
                end = end.AddDays(1);
                _context.Sittings.Add(new Sitting
                {
                    StartTime = start,
                    EndTime = end,
                    SittingType = sittingType,
                    RestaurantId = 1,
                    Active = active,
                    Capacity = capacity
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("SittingsCreated", new {id=sitting.Id});
        }
        //Sittings Created successful
        public IActionResult SittingsCreated(int id)
        {
            var sittingsCreated = _context.Sittings.First(r => r.Id == id);
            return View();
        }

        //Existing Sittings Delete
        [HttpGet]
        public async Task<IActionResult> SittingsDelete(int id)
        {
            var sittings = await _context.Sittings
                .Include(s => s.SittingType)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sittings == null)
            {
                return NotFound();
            }
            return View(sittings);
        }
        [HttpPost]
        [ActionName(nameof(SittingsDelete))]
        public async Task<IActionResult> SittingsDeleteConfirm(int id)
        {
            var sittings = await _context.Sittings
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sittings == null)
            {
                return NotFound();
            }
            _context.Sittings.Remove(sittings);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Existing Sittings Edit: not completed
        [HttpGet]
        public async Task<IActionResult> SittingsEdit(int id)
        {
            
            var sitting = await _context.Sittings
                .Include(s => s.Reservations)
                .Include(s => s.SittingType)
                .FirstOrDefaultAsync(s => s.Id == id);

            //var nullMsg = "This sitting has reservation, so can not be edit or delete.";

            if (sitting == null)
            {
                return NotFound();
            }

            //if (sitting.Main != null)
            //{
            //    return View(sitting);
            //} else
            //{
            //    return View(nullMsg);
            //};

            return View(sitting);
        }
        [HttpPost]
        public async Task<IActionResult> SittingsEdit(Sitting r)
        {
            var sitting = await _context.Sittings
                .FirstOrDefaultAsync(s => s.Id == r.Id);
            if(sitting == null)
            {
                return NotFound();
            }
            sitting.StartTime = r.StartTime;
            sitting.EndTime = r.EndTime;
            sitting.Capacity = r.Capacity;
            sitting.Active = r.Active;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = r.Id });
        }



    }
}

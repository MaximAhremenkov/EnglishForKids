using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EnglishForKids.Web.Controllers
{
    public class TopicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Topic/
        public async Task<IActionResult> Index()
        {
            var topics = await _context.Topics
                .Include(t => t.Category)
                .OrderBy(t => t.CategoryId)
                .ThenBy(t => t.Order)
                .ToListAsync();

            return View(topics);
        }

        // GET: /Topic/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.Rules)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }
    }
}
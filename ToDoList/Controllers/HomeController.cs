using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public ViewResult Index(string id)
        {
            // load current filters and data needed for filter drop downs in ViewBag
            //var filters = new Filters(id);
            //ViewBag.Filters = filters;
            //ViewBag.Categories = context.Categories.ToList();
            //ViewBag.Statuses = context.Statuses.ToList();
            //ViewBag.DueFilters = Filters.DueFilterValues;

            // load current filters and data needed for filter drop downs in ToDoViewModel
            var model = new ToDoViewModel
            {
                Filters = new Filters(id),
                Categories = context.Categories.ToList(),
                Statuses = context.Statuses.ToList(),
                DueFilters = Filters.DueFilterValues
            };


            // get open tasks from database based on current filters
            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (model.Filters.HasCategory) {
                query = query.Where(t => t.CategoryId == model.Filters.CategoryId);
            }
            if (model.Filters.HasStatus) {
                query = query.Where(t => t.StatusId == model.Filters.StatusId);
            }
            if (model.Filters.HasDue) {
                var today = DateTime.Today;
                if (model.Filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (model.Filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (model.Filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }
            model.Tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(model);
        }

        [HttpGet]
        public ViewResult Add()
        {
            //ViewBag.Categories = context.Categories.ToList();
            //ViewBag.Statuses = context.Statuses.ToList();
            //var task = new ToDo { StatusId = "open" };  // set default value for drop-down
            //return View(task);

            // Create a new ToDoViewModel
            var model = new ToDoViewModel
            {
                // Initialize properties of the view model
                Categories = context.Categories.ToList(),
                Statuses = context.Statuses.ToList(),
                DueFilters = Filters.DueFilterValues,
                CurrentTask = new ToDo { StatusId = "open" }  // Initialize with a default task
            };

            // Return the view with the model
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(ToDoViewModel model)
        {
            if (ModelState.IsValid)
            {
                //context.ToDos.Add(task);
                //context.SaveChanges();
                //return RedirectToAction("Index");

                // Add the current task to the database using the CurrentTask property of the model
                context.ToDos.Add(model.CurrentTask);
                context.SaveChanges();

                // Redirect to the Index page after successful task creation
                return RedirectToAction("Index");
            }
            else
            {
                //ViewBag.Categories = context.Categories.ToList();
                //ViewBag.Statuses = context.Statuses.ToList();
                //return View(task);

                // If the model is invalid, store the categories, statuses, and filters in the model
                model.Categories = context.Categories.ToList();
                model.Statuses = context.Statuses.ToList();
                model.DueFilters = Filters.DueFilterValues;

                // Return the view with the updated model
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute]string id, ToDo selected)
        {
            selected = context.ToDos.Find(selected.Id)!;  // use null-forgiving operator to suppress null warning
            if (selected != null)
            {
                selected.StatusId = "closed";
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult DeleteComplete(string id)
        {
            var toDelete = context.ToDos
                .Where(t => t.StatusId == "closed").ToList();

            foreach(var task in toDelete)
            {
                context.ToDos.Remove(task);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}
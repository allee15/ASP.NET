using ArticlesApp.Data;
using ArticlesApp.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace ArticlesApp.Controllers
{

    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public GroupsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;
            
            _roleManager = roleManager;
        }
        //[Authorize(Roles = "User,Editor,Admin")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            //var selectList = new List<SelectListItem>();
            //Group group = db.Groups.Include("UserInGroups").First();
            //group.UserInGroups = new List<UserInGroup>();
            //UserInGroup ug = new UserInGroup();
            //group.UserInGroups.Add(ug);
            //db.SaveChanges();

            // afisare grupuri impreuna cu categoria din care fac parte 
            var groups = db.Groups.Include("Category");
            // se va afisa si numele - face parte din Group
            var search = "";

            //motorul de cautare
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                // se elimina spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                
                // cautare in numele grupului
                List<int> groupIds = db.Groups.Where
                                     (
                                        gr =>gr.Name.Contains(search)
                                     )
                                    .Select(g => g.Id).ToList();
                // cautare in cadrul articolelor 
                List<int> groupsIdsOfCommentsWithSearchString = db.Comments
                                                                .Where
                                                                (
                                                                c => c.Content.Contains(search)
                                                                ).Select(c => c.Id).ToList(); // Id este Id-ul de la grup
                // formam o singura lista cu toate id-urile
                List<int> mergedIds = groupIds.Union(groupsIdsOfCommentsWithSearchString).ToList();

                groups = db.Groups.Where(group => mergedIds.Contains(group.Id))
                                        .Include("Category");


            }

            ViewBag.SearchString = search;

            //afisare paginata
            //cate 3 grupuri pe pagina
            int _perPage = 3;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            int totalItems = groups.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // offset = numarul de articole are au fost afisate pe paginile anterioare
            var offset = 0;

            // se calc offset
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            // preluare articole coreps in functie de offset
            var paginatedGroups = groups.Skip(offset).Take(_perPage);

            // numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            //trimitem grupurile cu un ViewBag
            ViewBag.Groups = paginatedGroups;

            if (search!= "")
            {
                ViewBag.PaginationBaseUrl = "/Groups/Index/?search=" + search + "&page";
            }
            else {

                ViewBag.PaginationBaseUrl = "Groups/Index/?page";
            }


            return View();

        }
        [HttpPost]
        public IActionResult Join(int IdGroup)
        {
            UserInGroup usrgr = new UserInGroup();
            usrgr.UserId = _userManager.GetUserId(User);
            usrgr.GroupId = IdGroup;

            var query = db.UserInGroups.Where(ugr => ugr.GroupId == IdGroup && ugr.UserId == usrgr.UserId);

            ViewBag.query = query;
            if (query != null)
            {
                TempData["message"] = "Sunteti deja in grup";
                
            }
            else
            {
                db.UserInGroups.Add(usrgr);
                db.SaveChanges();

            }
            //return RedirectToAction("Index");
            //ViewBag.query = "mama";
            //if (query == null)
            //{
            // in cazul in care nu este in baza de date, il adaugam iar 

            //return RedirectToAction("Index");
            // }

            return Redirect("/Groups/Show/" + IdGroup);
        }

        // 
        [Authorize (Roles = "User,Editor,Admin")]
        public IActionResult New()
        {
            Group group = new Group();
            group.Categ = GetAllCategories();
            //group.UserList = GetAllUsers();
            return View(group);
        }

        [Authorize (Roles = "User,Editor,Admin")]
        [HttpPost]
        public IActionResult New (Group group)
        {
            var sanitizer = new HtmlSanitizer();
            
            if (ModelState.IsValid)
            {
                group.Name = sanitizer.Sanitize(group.Name);
                group.UserId = _userManager.GetUserId(User);
                db.Groups.Add(group);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost adaugat";
                return RedirectToAction("Index");
            }
            else
            {
                group.Categ = GetAllCategories();
                group.UserList = GetAllUsers();
                return View(group);
            }
        }




        // Se afiseaza un singur grup in functie de id-ul sau 
        // impreuna cu categoria din care face parte si userii sai
        // In plus sunt preluate si toate comentariile asociate unui grup
       
        // HttpGet implicit
       
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {

            //get(group) with given id
            Group group = db.Groups.Include("Category")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(gr => gr.Id == id)
                                         .First();

            SetAccessRights();
            
            return View(group);
        }

        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Groups/Show/" + comment.GroupId);
            }

            else
            {
                Group group = db.Groups.Include("Category")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(gr => gr.Id == comment.GroupId)
                                         .First();

                SetAccessRights();

                return View(group);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Group group = db.Groups.Include("Category")
                                   .Where(gr => gr.Id == id)
                                   .First();
            group.Categ = GetAllCategories();


            // iau user-id-ul utilizatorului 
            //ApplicationUser usr = db.Groups.Include("ApplicationUser")
            //                               .Where(gr => gr.Id == id)
            //                               .First();

            if (group.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(group);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra grupului";
                return RedirectToAction("Index");
            }

        }

        // adaugare articol modificat in baza de date 
        // nu am adus modificari in ceea ce priveste userii
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Group requestGroup)
        {
            var sanitizer = new HtmlSanitizer();
            Group group = db.Groups.Find(id);
            if (ModelState.IsValid)
            {
                if (group.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    group.Name = requestGroup.Name;
                    group.CategoryId = requestGroup.CategoryId;
                    TempData["message"] = "Grupul a fost modificat";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui articol";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestGroup.Categ = GetAllCategories();
                return View(requestGroup);
            }
        }

        // se sterge un grup din baza de date 
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Group group = db.Groups.Include("Comments")
                            .Where(gr => gr.Id == id)
                            .First();

            var comments = db.Comments.Where(comm => comm.GroupId == id).ToList();
            var users = db.UserInGroups.Where(us => us.GroupId == id).ToList();

            if (User.IsInRole("Admin"))
            {
                db.Groups.Remove(group);

                if (comments != null)
                {
                    foreach (Comment comment in comments)
                    {
                        db.Comments.Remove(comment);
                    }
                    foreach (UserInGroup user in users)
                    {
                        db.UserInGroups.Remove(user);
                    }
                }
                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "NU aveti drepturi sa stergeti grupuri";
                return RedirectToAction("Index");
            }
        }


        
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            


            // returnam lista de categorii
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllUsers()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

           

            var users = from user in db.Users
                        orderby user.UserName
                        select user;

           
            foreach (var user in users)
            {
                // adaugam in lista elementele necesare pentru dropdown
                
                selectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                });
            }
            
            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor") || User.IsInRole("Admin"))
            {
                ViewBag.AfisareButoane = true;
            }

            //ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}

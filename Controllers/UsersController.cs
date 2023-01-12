using ArticlesApp.Data;
using ArticlesApp.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace ArticlesApp.Controllers
{
    [Authorize(Roles = "Admin,Editor,User")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

          
        

        public IActionResult Index()
        {
            string currentUserId = _userManager.GetUserId(User);
            ViewBag.CurrentUserId = currentUserId;
            
            /*var users = from user in db.Users
                        orderby user.UserName
                        
                        select user;
            */
            var users = db.Users.Where(us => us.Id != currentUserId);
            var search = "";

            // MOTOR DE CAUTARE
           

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in user (email)

                List<string> userIds = db.Users.Where
                                        (
                                         usr => usr.Email.Contains(search)
                                        
                                        ).Select(a => a.Id).ToList();


                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<string> mergedIds = userIds.Union(userIds).ToList();


                // Lista articolelor care contin cuvantul cautat

                users = (IOrderedQueryable<ApplicationUser>)db.Users.Where(user => mergedIds.Contains(user.Id));
                                      //.Include("UserName");

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Alegem sa afisam 5 users pe pagina
            int _perPage = 5;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            // Fiind un numar variabil de useri, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = users.Count();


            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedUsers = users.Skip(offset).Take(_perPage);


            // Preluam numarul ultimei pagini

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Articles = paginatedUsers;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Users/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Users/Index/?page";
            }


            //return View();

            ViewBag.UsersList = users;

            return View();
        }



        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            var currentUser = _userManager.GetUserId(User);
            // obtine cererile care sunt pe waiting
            var waitingList = db.Friendships.Where(rq => (rq.SenderUserId == id && rq.ReceiverUserId == currentUser)
                                                
                                                && rq.StatusCerere == 0); 

            
            ViewBag.WaitingList = waitingList;
            
            var areFriends = db.Friendships.Where(rq => ((rq.SenderUserId == currentUser && rq.ReceiverUserId == id)
                                                || (rq.SenderUserId == id && rq.ReceiverUserId == currentUser))
                                                && rq.StatusCerere == 1).FirstOrDefault();
            if (areFriends != null)
            {
                TempData["message"] = "Sunteti prieteni! ";
                ViewBag.message = TempData["message"].ToString();
                ViewBag.areFriends = true;
                return View(user);
            }
            else
            {
                ViewBag.areFriends = false;
            }

            // prietenii 
            ViewBag.CountWait = waitingList.Count();
            ApplicationUser userNow = db.Users.Where(u => u.Id == currentUser).First();

            ApplicationUser UserToSee = db.Users.Where(u => u.Id == id).FirstOrDefault(); 
            if (UserToSee.Privacy != "private")
            {
                ViewBag.IsPrivate = false;
                return View(user);
            }
            else
            {
                ViewBag.IsPrivate = true;
                TempData["message"] = "Contul este privat, deveniti prieteni si dupa accept il veti putea accesa";
                ViewBag.message = TempData["message"].ToString();
            }



            var req = db.Friendships.Where(rq => ((rq.SenderUserId == currentUser && rq.ReceiverUserId == id)
                                                || (rq.SenderUserId == id && rq.ReceiverUserId == currentUser))
                                                && rq.StatusCerere == 1)
                                       .FirstOrDefault(); 
            if (req != null || waitingList.Count()!=0)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");

            }

        }



        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            Console.WriteLine(id);
            Console.WriteLine("------------------------");
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

            // Cautam ID-ul rolului in baza de date
            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First(); // Selectam 1 singur rol
            
            ViewBag.UserRole = currentUserRole;
            

            if(user.Id == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(user);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa realizati modificari";
                return RedirectToAction("Index");
            }
            
        }

        [Authorize(Roles ="User,Admin")]
        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid && (newData.Privacy == "private" || newData.Privacy == "public"))
            {

                if (user.Id == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;
                    user.PhoneNumber = newData.PhoneNumber;
                    user.Privacy = newData.Privacy;

                    // Cautam toate rolurile din baza de date
                    var roles = db.Roles.ToList();

                    foreach (var role in roles)
                    {
                        // Scoatem userul din rolurile anterioare
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                    // Adaugam noul rol selectat
                    var roleName = await _roleManager.FindByIdAsync(newRole);
                    await _userManager.AddToRoleAsync(user, roleName.ToString());
                    TempData["message"] = "Modificare realizata";
                    db.SaveChanges();

                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa realizati modificari";
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                         .Include("Articles")
                         .Include("Comments")
                         .Include("Bookmarks")
                         .Where(u => u.Id == id)
                         .First();
             
            // Delete user articles
            if (user.Articles.Count > 0)
            {
                foreach (var article in user.Articles)
                {
                    db.Articles.Remove(article);
                }
            }
            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }
            // Delete user bookmarks
            if (user.Bookmarks.Count > 0)
            {
                foreach (var bookmark in user.Bookmarks)
                {
                    db.Bookmarks.Remove(bookmark);
                }
            }

            db.ApplicationUsers.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult SendRequest(string receiverId)
        {
            // sa adaug in try catch
            var currentUser = _userManager.GetUserId(User);
            var req = db.Friendships.Where(rq => ((rq.SenderUserId == currentUser && rq.ReceiverUserId == receiverId)
                                                || (rq.SenderUserId == receiverId && rq.ReceiverUserId == currentUser))
                                                )
                                       .FirstOrDefault();
            if (req == null) { 
            var friendship = new Friendship
            {
                DateJoined = DateTime.UtcNow,
                SenderUserId = _userManager.GetUserId(User),
                ReceiverUserId = receiverId,
                StatusCerere = 0 // waiting
            };
            db.Friendships.Add(friendship);
            db.SaveChanges();
                TempData["message"] = "Cerere trimisa";
                ViewBag.message = TempData["message"].ToString();
            }

            
            // sa il duc pe show
            return RedirectToAction("Index");
            
        }

        [HttpPost]
        public IActionResult Accept (string id)
        {
            Console.WriteLine("\nSender user");
            Console.WriteLine(id);
            Console.WriteLine("------------------\n");

           

            // cautare in tabela dupa Friendship
            var currentUser = _userManager.GetUserId(User);

            Console.WriteLine("\nReceiver user");
            Console.WriteLine(currentUser);
            Console.WriteLine("------------------\n");

            var find = db.Friendships.Where(fr => fr.SenderUserId == id && fr.ReceiverUserId == currentUser).FirstOrDefault();
            find.StatusCerere = 1;
            find.DateAccepted= DateTime.UtcNow;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(string id)
        {
            // cautare in tabela dupa Friendship
            var currentUser = _userManager.GetUserId(User);
            var find = db.Friendships.Where(fr => fr.SenderUserId == id && fr.ReceiverUserId == currentUser).FirstOrDefault();
            find.DateAccepted = DateTime.UtcNow;
            find.StatusCerere = 2;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPrivacy(string privacy)
        {
            if (privacy == "private" || privacy == "public") { 
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetUsernames(string id)
        {
            var selectList = new List<SelectListItem>();
            var currentUser = _userManager.GetUserId(User);
            var waitingList = db.Friendships.Where(rq => ((rq.SenderUserId == currentUser && rq.ReceiverUserId == id)
                                               || (rq.SenderUserId == id && rq.ReceiverUserId == currentUser))
                                               && rq.StatusCerere == 0);


            

            foreach (var user in waitingList)
            {
                var userName = db.ApplicationUsers.Where(us => us.Id == user.SenderUserId).FirstOrDefault();
                selectList.Add(new SelectListItem
                {
                    Value = userName.ToString(),
                    Text = userName.ToString()
                });
            }
            return selectList;
        }
    }
}

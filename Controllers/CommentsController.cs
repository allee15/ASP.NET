//using ArticlesApp.Data;
//using ArticlesApp.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ArticlesApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController (
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /*
        
        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if(ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

            else
            {
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

        }

        
        */


        // Stergerea unui comentariu asociat unui articol din baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                //if (comm.GroupId.IsNullOrEmpty())
                if (comm.GroupId == null)
                {
                    //inseamna ca este de la articol
                    return Redirect("/Articles/Show/" + comm.ArticleId);
                }
                else
                {
                    return Redirect("/Groups/Show/" + comm.GroupId);
                }
               
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi sa stergeti comentariul";
                return RedirectToAction("Index", "Articles");
            }

        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi sa stergeti comentariul";
                if (comm.GroupId == null)
                    return RedirectToAction("Index", "Articles");
                else
                {
                    return RedirectToAction("Index", "Groups");
                }
            }
                
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if(ModelState.IsValid)
            {

                if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    comm.Content = requestComment.Content;

                    db.SaveChanges();

                    if (comm.GroupId == null)
                        return Redirect("/Articles/Show/" + comm.ArticleId);
                    else
                    {
                        return Redirect("/Groups/Show/" + comm.GroupId);
                    }
                }
                else
                {
                    return View(requestComment);
                }
                    
            }
            else
            {
                
                TempData["message"] = "Nu aveti drepturi sa stergeti comentariul";
                if (comm.GroupId == null)
                    return RedirectToAction("Index", "Articles");
                else
                {
                    return RedirectToAction("Index", "Groups");
                }
            }

        }
    }
}
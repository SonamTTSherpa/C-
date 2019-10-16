using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private int? UserSession
        {
            get { return HttpContext.Session.GetInt32("UserId"); }
            set { HttpContext.Session.SetInt32("UserId", (int)value); }
        }
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("create")]
        public IActionResult CreateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists in db
                var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == newUser.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View("Index");
                }
                // Hash new user's password and save new user to db
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Users.Add(newUser);
                dbContext.SaveChanges();
                UserSession = newUser.UserId;
                return RedirectToAction("Dashboard", "Wedding");
            }
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser currUser)
        {
            if (ModelState.IsValid)
            {
                // Check if email exists in database
                var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == currUser.LoginEmail);
                if (existingUser == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(currUser, existingUser.Password, currUser.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                UserSession = existingUser.UserId;
                return RedirectToAction("Dashboard", "Wedding");
            }
            return View("Index");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}

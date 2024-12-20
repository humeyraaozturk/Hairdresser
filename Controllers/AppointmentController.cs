﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }
        public IActionResult Update()
        {
            return View();
        }
        public IActionResult Cancel()
        {
            return View();
        }
    }
}

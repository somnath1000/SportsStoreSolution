using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace SportsStoreWebApp.Controllers
{
  public class AboutController : Controller
  {
    private readonly IConfiguration _configuration;

    public AboutController(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public async Task<IActionResult> Index()
    {
      var keyVaultName = _configuration["SSKeyVault"];
      return View(new Dictionary<string, string>() { ["KeyVault"] = "Empty" });
    }
    public IActionResult Throw()
    {
      throw new EntryPointNotFoundException("This is a user thrown exception");
    }
  }
}


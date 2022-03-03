﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SportsStoreWebApp.Models.Abstract;
using SportsStoreWebApp.Models.Entities;

namespace SportsStoreWebApp.Controllers
{
  public class ProductController : Controller
  {
    private readonly IProductRepository _productRepository;
    private readonly IPhotoService _photoService;
    private readonly IConfiguration _configuration;

    public ProductController(IProductRepository productRepository, IPhotoService photoService, IConfiguration configuration)
    {
      _productRepository = productRepository;
      _photoService = photoService;
      _configuration = configuration;
    }

    public IActionResult Index() => View();
    public async Task<IActionResult> List()
    {
      var productsList = await _productRepository.GetAllProductsAsync();
      return View(productsList);
    }

    public ActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(include: "ProductName, Description, Price, Category, PhotoUrl")] Product product, IFormFile photo)
    {
      if (ModelState.IsValid)
      {
        product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
        await _productRepository.CreateAsync(product);
        TempData["newproduct"] = $"New Product: '{product.ProductName}' with Id '{product.ProductId}' has been added successfully";
        return RedirectToAction("List");
      }
      return View(product);
    }

    public async Task<ActionResult> GetByCategory(string category)
    {
      ViewBag.category = category;
      var result = await _productRepository.FindProductsByCategoryAsync(category);
      return View(result);
    }

    public async Task<ActionResult> Edit(int productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      return View(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Product product, IFormFile photo)
    {
      if (photo == null) { }
      else
      {
        if (await _photoService.DeletePhotoAsync(product.Category, product.PhotoUrl))
        {
          product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
        }
      }
      await _productRepository.UpdateAsync(product);
      return RedirectToAction("List");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      if (await _photoService.DeletePhotoAsync(result.Category, result.PhotoUrl))
      {
        await _productRepository.DeleteAsync(productId);
      }
      return RedirectToAction("List");
    }

    public IActionResult Contact() => View();

    public IActionResult ClearCache()
    {
      _productRepository.ClearCache();
      return RedirectToAction("List");
    }
  }
}


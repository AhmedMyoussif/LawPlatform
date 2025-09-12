using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LawPlatform.DataAccess.Services.Category;
using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Entities.Shared.Bases;
using Ecommerce.DataAccess.Services.Category;

namespace LawPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ResponseHandler _responseHandler;

        public CategoryController(ICategoryService categoryService, ResponseHandler responseHandler)
        {
            _categoryService = categoryService;
            _responseHandler = responseHandler;
        }

        [HttpPost("Add/Category")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateCategoryRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

            var result = await _categoryService.AddCategoryAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("byName/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            return StatusCode((int)result.StatusCode, result);
        }

        

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCategoryRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}

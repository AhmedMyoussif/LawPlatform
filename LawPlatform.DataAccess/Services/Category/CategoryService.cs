using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Shared.Bases;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using LawPlatform.DataAccess.Services.Category;
using LawPlatform.DataAccess.Services.Category;

namespace LawPlatform.DataAccess.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly LawPlatformContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(LawPlatformContext context, ResponseHandler responseHandler, ILogger<CategoryService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        private async Task<GetCategoryResponse?> GetCategoryAsync(Expression<Func<ConsultationCategory, bool>> predicate)
        {
            return await _context.ConsultationCategories
                .Where(predicate)
                .Select(c => new GetCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Response<GetCategoryResponse>> AddCategoryAsync(CreateCategoryRequest dto)
        {
            _logger.LogInformation("AddCategoryAsync called with Name={CategoryName}", dto?.Name);

            if (dto == null)
            {
                _logger.LogWarning("AddCategoryAsync called with null data.");
                return _responseHandler.BadRequest<GetCategoryResponse>("Category data is required.");
            }

            var existingCategory = await _context.ConsultationCategories
                .FirstOrDefaultAsync(c => c.Name == dto.Name && !c.IsDeleted);

            if (existingCategory != null)
            {
                return _responseHandler.BadRequest<GetCategoryResponse>("A category with the same name already exists.");
            }

            var category = new ConsultationCategory
            {                
                Name = dto.Name,
                Description = dto.Description,
                IsDeleted = false
            };

            await _context.ConsultationCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New category created: {Name}", dto.Name);

            return _responseHandler.Created<GetCategoryResponse>(
                new GetCategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                },
                "Category created successfully."
            );
        }

        public async Task<Response<List<GetCategoryResponse>>> GetAllCategoriesAsync()
        {
            var categories = await _context.ConsultationCategories
                .Where(c => !c.IsDeleted)
                .Select(c => new GetCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                })
                .ToListAsync();

            return _responseHandler.Success(categories, "Categories retrieved successfully.");
        }

        public async Task<Response<GetCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            if (id == null)
            {
                _logger.LogWarning("GetCategoryByIdAsync called with empty ID.");
                return _responseHandler.BadRequest<GetCategoryResponse>("Invalid category ID.");
            }

            var category = await GetCategoryAsync(c => c.Id == id && !c.IsDeleted);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found.", id);
                return _responseHandler.NotFound<GetCategoryResponse>("Category not found.");
            }

            return _responseHandler.Success(category, "Category retrieved successfully.");
        }

        public async Task<Response<GetCategoryResponse>> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetCategoryByNameAsync called with empty name.");
                return _responseHandler.BadRequest<GetCategoryResponse>("Invalid category name.");
            }

            var category = await GetCategoryAsync(c => c.Name == name && !c.IsDeleted);
            if (category == null)
            {
                _logger.LogWarning("Category with name {Name} not found.", name);
                return _responseHandler.NotFound<GetCategoryResponse>("Category not found.");
            }

            return _responseHandler.Success(category, "Category retrieved successfully.");
        }

        public async Task<Response<ConsultationCategory>> DeleteCategoryAsync(Guid id)
        {
            if (id == null)
            {
                _logger.LogWarning("DeleteCategoryAsync called with empty ID.");
                return _responseHandler.BadRequest<ConsultationCategory>("Invalid category ID.");
            }

            var category = await _context.ConsultationCategories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                _logger.LogWarning("DeleteCategoryAsync - Category not found or already deleted. ID: {Id}", id);
                return _responseHandler.NotFound<ConsultationCategory>("Category not found or already deleted.");
            }

            // Soft delete
            category.IsDeleted = true;
            _context.ConsultationCategories.Update(category);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category with ID {Id} was deleted successfully.", id);

            return _responseHandler.Success(category, "Category deleted successfully.");
        }

        public async Task<Response<Guid>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest dto)
        {
            if (id == null || dto == null)
            {
                _logger.LogWarning("UpdateCategoryAsync called with invalid input. ID: {Id}", id);
                return _responseHandler.BadRequest<Guid>("Invalid input data.");
            }

            var category = await _context.ConsultationCategories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                _logger.LogWarning("UpdateCategoryAsync - Category not found. ID: {Id}", id);
                return _responseHandler.NotFound<Guid>("Category not found.");
            }

            if (category.Name == dto.Name && category.Description == dto.Description)
            {
                return _responseHandler.BadRequest<Guid>("No changes detected.");
            }

            // Check for duplication with another category (excluding current one)
            var existingCategory = await _context.ConsultationCategories
                .FirstOrDefaultAsync(c =>
                    c.Id != id &&
                    c.Name == dto.Name &&
                    !c.IsDeleted
                );

            if (existingCategory != null)
            {
                return _responseHandler.BadRequest<Guid>("Another category with the same name already exists.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            _context.ConsultationCategories.Update(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category with ID {Id} updated successfully.", id);

            return _responseHandler.Success<Guid>(category.Id, "Category updated successfully.");
        }
    }
}

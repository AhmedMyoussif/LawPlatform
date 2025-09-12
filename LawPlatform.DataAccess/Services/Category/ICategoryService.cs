using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LawPlatform.Entities.DTO.Category;
using LawPlatform.Entities.Shared.Bases;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.DTO.Category;

namespace LawPlatform.DataAccess.Services.Category
{
    public interface ICategoryService
    {
        Task<Response<GetCategoryResponse>> AddCategoryAsync(CreateCategoryRequest dto);
        Task<Response<List<GetCategoryResponse>>> GetAllCategoriesAsync();

        Task<Response<GetCategoryResponse>> GetCategoryByIdAsync(string id);
        Task<Response<GetCategoryResponse>> GetCategoryByNameAsync(string name);
        Task<Response<ConsultationCategory>> DeleteCategoryAsync(string id);
        Task<Response<string>> UpdateCategoryAsync(string id, UpdateCategoryRequest dto);
    }
}

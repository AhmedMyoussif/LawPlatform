using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Category
{
    public class CreateCategoryRequest
    {
   
        public string Name { get; set; }
        public string Description { get; set; }

    }
}

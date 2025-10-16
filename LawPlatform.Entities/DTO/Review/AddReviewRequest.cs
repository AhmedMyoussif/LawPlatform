using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.DTO.Review;

public record AddReviewRequest(
    string Comment,
    double Rating,
    Guid LawyerId
);

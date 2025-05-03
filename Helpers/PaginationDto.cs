﻿using System.ComponentModel.DataAnnotations;

namespace e_learning.Helpers
{
    public class PaginationDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}

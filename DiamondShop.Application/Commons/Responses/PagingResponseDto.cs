﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Commons.Responses
{
    public record PagingResponseDto<T> (int TotalPage, int CurrentPage, List<T> Values,int totalCount = 0, int totalTake = 0);

}

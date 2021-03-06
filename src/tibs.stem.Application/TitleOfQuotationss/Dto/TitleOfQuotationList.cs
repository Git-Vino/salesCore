﻿using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using tibs.stem.TitleOfQuotations;

namespace tibs.stem.TitleOfQuotationss.Dto
{
    [AutoMapFrom(typeof(TitleOfQuotation))]
    public class TitleOfQuotationList
    {
        public int Id { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public int TenantId { get; set; }
    }
}

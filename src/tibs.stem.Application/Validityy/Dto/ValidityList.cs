﻿using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using tibs.stem.Validitys;

namespace tibs.stem.Validityy.Dto
{
    [AutoMapFrom(typeof(Validity))]
    public class ValidityList
    {
        public int Id { get; set; }
        public virtual string ValidityCode { get; set; }
        public virtual string ValidityName { get; set; }
        public int TenantId { get; set; }
    }
}
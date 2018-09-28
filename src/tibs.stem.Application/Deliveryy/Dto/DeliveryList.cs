﻿using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using tibs.stem.Deliverys;

namespace tibs.stem.Deliveryy.Dto
{
    [AutoMapFrom(typeof(Delivery))]
    public class DeliveryList
    {
        public int Id { get; set; }
        public virtual string DeliveryCode { get; set; }
        public virtual string DeliveryName { get; set; }
        public int TenantId { get; set; }
    }
}
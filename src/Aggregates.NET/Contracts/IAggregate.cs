﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface IAggregate : IEntity, IHaveEntities
    {
    }

    public interface IAggregate<TId> : IAggregate, IEntity<TId>
    {
        TId Id { get; }
    }
}
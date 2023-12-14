﻿using Pathfinding.App.Console.DataAccess.Repos;
using System;

namespace Pathfinding.App.Console.DataAccess.UnitOfWorks
{
    internal interface IUnitOfWork : IDisposable
    {
        IGraphRepository GraphRepository { get; }

        IAlgorithmsRepository AlgorithmsRepository { get; }

        IVerticesRepository VerticesRepository { get; }

        INeighborsRepository NeighborsRepository { get; }

        IRangeRepository RangeRepository { get; }

        void SaveChanges();

        void BeginTransaction();

        void RollbackTransaction();

        void CommitTransaction();
    }
}

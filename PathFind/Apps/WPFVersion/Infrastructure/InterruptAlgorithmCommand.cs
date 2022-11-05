﻿using Algorithm.Base;

namespace WPFVersion.Infrastructure
{
    internal sealed class InterruptAlgorithmCommand : BaseAlgorithmCommand
    {
        public InterruptAlgorithmCommand(PathfindingProcess algorithm) : base(algorithm)
        {

        }

        public override bool CanExecute(object parameter)
        {
            return algorithm.IsInProcess;
        }

        public override void Execute(object parameter)
        {
            algorithm.Interrupt();
        }
    }
}

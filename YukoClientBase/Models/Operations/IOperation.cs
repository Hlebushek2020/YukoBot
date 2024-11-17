using System;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;

namespace YukoClientBase.Models.Operations
{
    public interface IOperation
    {
        Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken);
    }
}
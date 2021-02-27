// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Watcher.Internal;

namespace Microsoft.DotNet.Watcher.Tools
{
    /// <summary>
    /// When using hot reload, we have to ensure PDBs are correct before we initialize the MSBuild workspace.
    /// This filter ensures the state by first building the app.
    /// </summary>
    public class InitialBuildFilter : IWatchFilter
    {
        private readonly ProcessRunner _processRunner;

        public InitialBuildFilter(ProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public async ValueTask ProcessAsync(DotNetWatchContext context, CancellationToken cancellationToken)
        {
            if (context.Iteration == 0 && context.ProcessSpec.Arguments.Count == 1 && context.ProcessSpec.Arguments[0] == "run")
            {
                var processSpec = new ProcessSpec
                {
                    Executable = context.ProcessSpec.Executable,
                    Arguments = new[] { "build" },
                    WorkingDirectory = context.ProcessSpec.WorkingDirectory,
                };

                await _processRunner.RunAsync(processSpec, cancellationToken);

                context.ProcessSpec.Arguments = context.ProcessSpec.Arguments.Append("--no-build").ToArray();
            }

        }
    }
}

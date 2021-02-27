// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.EditAndContinue;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Tools.Internal;
using Microsoft.VisualStudio.Debugger.Contracts.EditAndContinue;


namespace Microsoft.DotNet.Watcher.Tools
{
    public class CompilationHandler
    {
        private static readonly SolutionActiveStatementSpanProvider NoActiveSpans = (_, _) => new(ImmutableArray<TextSpan>.Empty); 
        private Task<(MSBuildWorkspace, IEditAndContinueWorkspaceService)> _initializeTask;
        private bool _failedToInitialize;
        private readonly IReporter _reporter;

        public CompilationHandler(IReporter reporter)
        {
            _reporter = reporter;
        }

        public async ValueTask InitializeAsync(DotNetWatchContext context)
        {
            if (context.Iteration == 0)
            {
                var instance = MSBuildLocator.QueryVisualStudioInstances().First();

                _reporter.Verbose($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
                MSBuildLocator.RegisterInstance(instance);
            }
            else if (_initializeTask is not null)
            {
                var (msw, project) = await _initializeTask;
                msw.Dispose();
                _initializeTask = null;
            }

            if (context.FileSet.IsNetCoreApp31OrNewer) // needs to be net6.0
            {
                // Todo: figure this out for multi-project workspaces
                var project = context.FileSet.First(f => f.FilePath.EndsWith(".csproj", StringComparison.Ordinal));
                _initializeTask = CreateMSBuildProjectAsync(project.FilePath, _reporter);
                await _initializeTask;

                context.ProcessSpec.EnvironmentVariables["COMPLUS_ForceEnc"] = "1";
            }

            return;
        }

        public async ValueTask<bool> TryHandleFileChange(DotNetWatchContext context, FileItem file, CancellationToken cancellationToken)
        {
            if (!file.FilePath.EndsWith(".cs", StringComparison.Ordinal) &&
                !file.FilePath.EndsWith(".razor", StringComparison.Ordinal))
            {
                return false;
            }

            var (workspace, editAndContinue) = await _initializeTask;

            if (_failedToInitialize)
            {
                return false;
            }

            var documentToUpdate = workspace.CurrentSolution.Projects.SelectMany(p => p.Documents).FirstOrDefault(d => d.FilePath == file.FilePath);
            editAndContinue.StartEditSession(StubDebuggerService.Instance, out _);
            if (documentToUpdate != null)
            {
                Console.WriteLine(documentToUpdate.GetTextSynchronously(default));

                var text = await ReadFileTextWithRetry(file.FilePath);

                var updatedDocument = documentToUpdate.WithText(SourceText.From(text, Encoding.UTF8));

                if (!workspace.TryApplyChanges(updatedDocument.Project.Solution))
                {
                    _reporter.Verbose("Unable to apply changes.");
                    return false;
                }
            }

            var (updates, diagnostics) = await editAndContinue.EmitSolutionUpdateAsync(workspace.CurrentSolution, NoActiveSpans, cancellationToken);

            if (updates.Status != ManagedModuleUpdateStatus.Ready)
            {
                _reporter.Verbose("Unable to apply update.");
                foreach (var diagnosticData in diagnostics)
                {
                    var project = workspace.CurrentSolution.GetProject(diagnosticData.ProjectId);
                    var diagnostic = await diagnosticData.ToDiagnosticAsync(project, cancellationToken);
                    _reporter.Verbose(diagnostic.ToString());
                }

                return false;
            }

            editAndContinue.CommitSolutionUpdate();
            editAndContinue.EndEditSession(out _);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(new UpdatePayload
            {
                Deltas = updates.Updates.Select(c => new UpdateDelta
                {
                    ILDelta = c.ILDelta.ToArray(),
                    MetadataDelta = c.MetadataDelta.ToArray(),
                })
            });

            await context.BrowserRefreshServer.SendMessage(bytes);

            return true;
        }

        private async Task<(MSBuildWorkspace, IEditAndContinueWorkspaceService)> CreateMSBuildProjectAsync(string projectPath, IReporter reporter)
        {
            var msw = MSBuildWorkspace.Create();

            msw.WorkspaceFailed += (_sender, diag) =>
            {
                if (diag.Diagnostic.Kind == WorkspaceDiagnosticKind.Warning)
                {
                    reporter.Verbose($"MSBuildWorkspace warning: {diag.Diagnostic}");
                }
                else
                {
                    reporter.Warn($"Failed to create MSBuildWorkspace: {diag.Diagnostic}");
                    _failedToInitialize = true;
                }
            };

            var enc = msw.Services.GetRequiredService<IEditAndContinueWorkspaceService>();
            await msw.OpenProjectAsync(projectPath);
            enc.StartDebuggingSession(msw.CurrentSolution);

            foreach (var project in msw.CurrentSolution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    await document.GetTextAsync();
                    await enc.OnSourceFileUpdatedAsync(document);
                }
            }

            _failedToInitialize = false;

            return (msw, enc);
        }

        private static async ValueTask<string> ReadFileTextWithRetry(string path)
        {
            for (var attemptIndex = 0; attemptIndex < 10; attemptIndex++)
            {
                try
                {
                    return File.ReadAllText(path);
                }
                // Presumably this is not the right way to handle this
                catch (IOException) when (attemptIndex < 8)
                {
                    await Task.Delay(100);
                }
            }

            Debug.Fail("This shouldn't happen.");
            return null;
        }

        private readonly struct UpdatePayload
        {
            public string Type => "UpdateCompilation";

            public IEnumerable<UpdateDelta> Deltas { get; init; }
        }

        private readonly struct UpdateDelta
        {
            public string ModulePath { get; init; }
            public byte[] MetadataDelta { get; init; }
            public byte[] ILDelta { get; init; }
        }
    }
}

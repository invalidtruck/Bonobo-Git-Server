﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.Durability
{
    /// <summary>
    /// provides durability for result of git command execution
    /// by writing result of git command to a file
    /// </summary>
    public class DurableGitServiceResult : IGitService
    {
        private readonly IGitService gitService;
        private readonly IResultFilePathBuilder resultFilePathBuilder;

        public DurableGitServiceResult(IGitService gitService, IResultFilePathBuilder resultFilePathBuilder)
        {
            this.gitService = gitService;
            this.resultFilePathBuilder = resultFilePathBuilder;
        }

        public void ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, System.IO.Stream inStream, System.IO.Stream outStream)
        {
            var resultFilePath = resultFilePathBuilder.GetPathToResultFile(correlationId, repositoryName, serviceName);
            using (var resultFileStream = File.OpenWrite(resultFilePath))
            {
                this.gitService.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, new ReplicatingStream(outStream, resultFileStream));
            }

            // only on successful execution remove the result file
            if(File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }
        }
    }
}
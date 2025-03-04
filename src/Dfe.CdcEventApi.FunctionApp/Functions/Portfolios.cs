﻿namespace Dfe.CdcEventApi.FunctionApp.Functions
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.CdcEventApi.Application.Definitions;
    using Dfe.CdcEventApi.Application.Models;
    using Dfe.CdcEventApi.Domain.Definitions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Entry class for the <c>portfolios</c> function.
    /// </summary>
    public class Portfolios : FunctionsBase
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Portfolios" />
        /// class.
        /// </summary>
        /// <param name="entityProcessor">
        /// An instance of type <see cref="IEntityProcessor" />.
        /// </param>
        /// <param name="loggerProvider">
        /// An instance of type <see cref="ILoggerProvider" />.
        /// </param>
        public Portfolios(
            IEntityProcessor entityProcessor,
            ILoggerProvider loggerProvider)
            : base(
                  entityProcessor,
                  loggerProvider)
        {
            // Nothing for now.
        }

        /// <summary>
        /// Entry method for the <c>portfolios</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of type <see cref="HttpRequest" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="HttpResponseMessage" />.
        /// </returns>
        [FunctionName("portfolios")]
        public async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "POST")]
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage toReturn =
                await this.RunAsync<Portfolio>(
                    httpRequest,
                    cancellationToken)
                .ConfigureAwait(false);

            return toReturn;
        }
    }
}
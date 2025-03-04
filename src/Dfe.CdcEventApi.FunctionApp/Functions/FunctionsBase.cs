﻿namespace Dfe.CdcEventApi.FunctionApp.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.CdcEventApi.Application.Definitions;
    using Dfe.CdcEventApi.Application.Exceptions;
    using Dfe.CdcEventApi.Application.Models;
    using Dfe.CdcEventApi.Domain.Definitions;
    using Dfe.CdcEventApi.Domain.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract base class for all functions.
    /// </summary>
    public abstract class FunctionsBase
    {
        private const string HeaderNameRunIdentifier = "X-Run-Identifier";

        private readonly IEntityProcessor entityProcessor;
        private readonly ILoggerProvider loggerProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="FunctionsBase" />
        /// class.
        /// </summary>
        /// <param name="entityProcessor">
        /// An instance of type <see cref="IEntityProcessor" />.
        /// </param>
        /// <param name="loggerProvider">
        /// An instance of type <see cref="ILoggerProvider" />.
        /// </param>
        public FunctionsBase(
            IEntityProcessor entityProcessor,
            ILoggerProvider loggerProvider)
        {
            this.entityProcessor = entityProcessor;
            this.loggerProvider = loggerProvider;
        }

        /// <summary>
        /// Base entry method for all functions.
        /// </summary>
        /// <typeparam name="TModelsBase">
        /// A type deriving from <see cref="ModelsBase" />.
        /// </typeparam>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpRequest" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="HttpResponseMessage" />.
        /// </returns>
        protected async Task<HttpResponseMessage> RunAsync<TModelsBase>(
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
            where TModelsBase : ModelsBase
        {
            HttpResponseMessage toReturn = null;

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            this.loggerProvider.Debug(
                $"Checking for header \"{HeaderNameRunIdentifier}\"...");

            IHeaderDictionary headerDictionary = httpRequest.Headers;

            DateTime? runIdentifier = null;
            string runIdentifierStr = null;
            if (headerDictionary.ContainsKey(HeaderNameRunIdentifier))
            {
                runIdentifierStr = headerDictionary[HeaderNameRunIdentifier];

                this.loggerProvider.Info(
                    $"Header \"{HeaderNameRunIdentifier}\" was specified: " +
                    $"\"{runIdentifierStr}\". Parsing...");

                try
                {
                    runIdentifier = DateTime.Parse(
                        runIdentifierStr,
                        CultureInfo.InvariantCulture);
                }
                catch (FormatException formatException)
                {
                    this.loggerProvider.Warning(
                        $"Unable to parse the value of " +
                        $"\"{HeaderNameRunIdentifier}\" " +
                        $"(\"{runIdentifierStr}\") as a {nameof(DateTime)}.",
                        formatException);
                }
            }

            if (string.IsNullOrEmpty(runIdentifierStr))
            {
                runIdentifier = DateTime.UtcNow;

                this.loggerProvider.Info(
                    $"Header \"{HeaderNameRunIdentifier}\" not supplied, or " +
                    $"was blank. {nameof(runIdentifierStr)} will default to " +
                    $"\"{runIdentifierStr}\".");
            }

            if (runIdentifier.HasValue)
            {
                string body = await httpRequest.ReadAsStringAsync()
                    .ConfigureAwait(false);

                string modelsBaseType = typeof(TModelsBase).Name;

                this.loggerProvider.Debug(
                    $"Deserialising received body: \"{body}\" into an array " +
                    $"of {modelsBaseType} instance(s)...");

                IEnumerable<TModelsBase> modelsBases =
                    JsonConvert.DeserializeObject<IEnumerable<TModelsBase>>(body);

                this.loggerProvider.Info(
                    $"{modelsBases.Count()} {modelsBaseType} instance(s) " +
                    $"deserialised.");

                this.loggerProvider.Debug(
                    $"Passing {modelsBases.Count()} entities to the entity " +
                    $"processor...");

                try
                {
                    await this.entityProcessor.ProcessEntitiesAsync(
                        runIdentifier.Value,
                        modelsBases,
                        cancellationToken)
                        .ConfigureAwait(false);

                    this.loggerProvider.Info(
                        $"All {modelsBases.Count()} entities processed.");

                    // Everything good? Return accepted.
                    toReturn =
                        new HttpResponseMessage(HttpStatusCode.Accepted);

                    // Also return the run identifier.
                    runIdentifierStr = runIdentifier.ToString();

                    toReturn.Headers.Add(
                        HeaderNameRunIdentifier,
                        runIdentifierStr);
                }
                catch (MissingDataHandlerAttributeException missingDataHandlerAttributeException)
                {
                    toReturn = new HttpResponseMessage(
                        HttpStatusCode.NotImplemented);

                    this.loggerProvider.Error(
                        $"A {nameof(MissingDataHandlerAttributeException)} " +
                        $"was thrown, returning " +
                        $"{HttpStatusCode.NotImplemented}. Message: " +
                        $"{missingDataHandlerAttributeException.Message}",
                        missingDataHandlerAttributeException);
                }
                catch (MissingDataHandlerFileException missingDataHandlerFileException)
                {
                    toReturn = new HttpResponseMessage(
                        HttpStatusCode.NotImplemented);

                    this.loggerProvider.Error(
                        $"A {nameof(MissingDataHandlerFileException)} was " +
                        $"thrown, returning " +
                        $"{HttpStatusCode.NotImplemented}. Message: " +
                        $"{missingDataHandlerFileException.Message}",
                        missingDataHandlerFileException);
                }
            }
            else
            {
                toReturn = new HttpResponseMessage(HttpStatusCode.BadRequest);

                this.loggerProvider.Error(
                    $"A valid {nameof(runIdentifier)} was not supplied. The " +
                    $"{nameof(runIdentifier)} should either not be " +
                    $"specified, or be a valid {nameof(DateTime)} value.");
            }

            return toReturn;
        }
    }
}
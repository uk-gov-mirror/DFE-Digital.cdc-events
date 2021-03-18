﻿namespace Dfe.CdcEventApi.Application.Models
{
    using System.Collections.Generic;
    using Dfe.CdcEventApi.Application.Definitions;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an individual <c>actuals</c> entity.
    /// </summary>
    [DataHandler("Create_Raw_Actual")]
    public class Actual : ModelsBase
    {
        /// <summary>
        /// Gets or sets the <c>Fields</c> property.
        /// </summary>
        [DataHandler("Create_Raw_Actual_Field")]
        [JsonProperty("Fields")]
        public IEnumerable<CustomField> Fields
        {
            get;
            set;
        }
    }
}
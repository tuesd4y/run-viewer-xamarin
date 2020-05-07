namespace RuntasticViewer.Json
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class RuntasticTrace
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("features")] public Feature[] Features { get; set; }
    }

    public partial class Feature
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("properties")] public Properties Properties { get; set; }

        [JsonProperty("geometry")] public Geometry Geometry { get; set; }
    }

    public partial class Geometry
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("coordinates")] public double[][] Coordinates { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("time")] public DateTimeOffset Time { get; set; }

        [JsonProperty("links")] public Link[] Links { get; set; }

        [JsonProperty("coordTimes")] public DateTimeOffset[] CoordTimes { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("href")] public Uri Href { get; set; }

        [JsonProperty("text")] public string Text { get; set; }
    }
}
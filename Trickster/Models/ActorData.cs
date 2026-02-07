// Models/ActorData.cs

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trickster.Models;

[Serializable]
public class ActorData
{
  [JsonProperty("id")]
  public string Id { get; set; } = "";
    
  [JsonProperty("displayName")]
  public string DisplayName { get; set; } = "";
    
  [JsonProperty("textures")]
  public Dictionary<string, ActorTextures> Textures { get; set; } = new();
}

[Serializable]
public class ActorTextures
{
  [JsonProperty("atlasPath")]
  public string AtlasPath { get; set; } = "";
    
  [JsonProperty("atlasRegions")]
  public Dictionary<string, RectLike> AtlasRegions { get; set; } = new();
}

[Serializable]
public class RectLike
{
  [JsonProperty("x")]
  public float X { get; set; }
    
  [JsonProperty("y")]
  public float Y { get; set; }
    
  [JsonProperty("width")]
  public float Width { get; set; }
    
  [JsonProperty("height")]
  public float Height { get; set; }
}

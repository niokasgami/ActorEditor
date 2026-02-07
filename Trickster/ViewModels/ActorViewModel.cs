// ViewModels/ActorItemViewModel.cs

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Trickster.Models;

namespace Trickster.ViewModels;

public partial class ActorItemViewModel : ObservableObject
{
  public ActorData Data { get; }
    
  public ActorItemViewModel(ActorData data)
  {
    Data = data;
    RefreshTextureKeys();
  }
    
  public string Id
  {
    get => Data.Id;
    set
    {
      if (Data.Id != value)
      {
        Data.Id = value;
        OnPropertyChanged();
      }
    }
  }
    
  public string DisplayName
  {
    get => Data.DisplayName;
    set
    {
      if (Data.DisplayName != value)
      {
        Data.DisplayName = value;
        OnPropertyChanged();
      }
    }
  }
    
  [ObservableProperty]
  private ObservableCollection<string> textureKeys = new();
    
  public void RefreshTextureKeys()
  {
    TextureKeys.Clear();
    foreach (var key in Data.Textures.Keys)
    {
      TextureKeys.Add(key);
    }
  }
}

public partial class TextureItemViewModel : ObservableObject
{
  public ActorTextures Data { get; }
    
  public TextureItemViewModel(ActorTextures data)
  {
    Data = data;
  }
    
  public string AtlasPath
  {
    get => Data.AtlasPath;
    set
    {
      if (Data.AtlasPath != value)
      {
        Data.AtlasPath = value;
        OnPropertyChanged();
      }
    }
  }
    
  public Dictionary<string, RectLike> AtlasRegions => Data.AtlasRegions;
}

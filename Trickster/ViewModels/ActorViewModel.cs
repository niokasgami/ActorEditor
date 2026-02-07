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
        RefreshAtlasRegions();
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
    
    [ObservableProperty]
    private ObservableCollection<AtlasRegionViewModel> atlasRegions = new();
    
    public void RefreshAtlasRegions()
    {
        AtlasRegions.Clear();
        foreach (var kvp in Data.AtlasRegions)
        {
            AtlasRegions.Add(new AtlasRegionViewModel(kvp.Key, kvp.Value, Data));
        }
    }
}

public partial class AtlasRegionViewModel : ObservableObject
{
    private readonly RectLike _data;
    private readonly ActorTextures _parentTextures;
    private string _key;
    
    public AtlasRegionViewModel(string key, RectLike data, ActorTextures parentTextures)
    {
        _key = key;
        _data = data;
        _parentTextures = parentTextures;
    }
    
    public string Key
    {
        get => _key;
        set
        {
            if (_key != value && !string.IsNullOrWhiteSpace(value))
            {
                // Check if the new name already exists
                if (_parentTextures.AtlasRegions.ContainsKey(value))
                {
                    // Can't rename to an existing key
                    return;
                }
                
                // Remove old key and add with new key
                _parentTextures.AtlasRegions.Remove(_key);
                _parentTextures.AtlasRegions.Add(value, _data);
                _key = value;
                OnPropertyChanged();
            }
        }
    }
    
    public float X
    {
        get => _data.X;
        set
        {
            if (_data.X != value)
            {
                _data.X = value;
                OnPropertyChanged();
            }
        }
    }
    
    public float Y
    {
        get => _data.Y;
        set
        {
            if (_data.Y != value)
            {
                _data.Y = value;
                OnPropertyChanged();
            }
        }
    }
    
    public float Width
    {
        get => _data.Width;
        set
        {
            if (_data.Width != value)
            {
                _data.Width = value;
                OnPropertyChanged();
            }
        }
    }
    
    public float Height
    {
        get => _data.Height;
        set
        {
            if (_data.Height != value)
            {
                _data.Height = value;
                OnPropertyChanged();
            }
        }
    }
}

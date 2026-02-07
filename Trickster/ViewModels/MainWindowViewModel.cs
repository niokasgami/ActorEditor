

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Trickster.Models;

namespace Trickster.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ActorItemViewModel? selectedActor;
    
    [ObservableProperty]
    private string? selectedTextureName;
    
    [ObservableProperty]
    private TextureItemViewModel? selectedTexture;
    
    [ObservableProperty]
    private string statusMessage = "Ready";
    
    [ObservableProperty]
    private string projectRootPath = ""; // This will store the root path for relative paths
    
    private string? currentFilePath;
    
    public MainViewModel()
    {
        // Load sample data matching your JSON structure
        var sampleActor = new ActorData
        {
            Id = "tomo",
            DisplayName = "Tomo",
            Textures = new Dictionary<string, ActorTextures>
            {
                ["shizu_minigame"] = new ActorTextures
                {
                    AtlasPath = "tomo_shizu_minigame.png",
                    AtlasRegions = new Dictionary<string, RectLike>
                    {
                        ["normal"] = new RectLike { X = 0, Y = 0, Width = 128, Height = 128 },
                        ["stage_01"] = new RectLike { X = 0, Y = 128, Width = 128, Height = 128 }
                    }
                }
            }
        };
        
        Actors.Add(new ActorItemViewModel(sampleActor));
        SelectedActor = Actors.FirstOrDefault();
    }
    
    public ObservableCollection<ActorItemViewModel> Actors { get; } = new();
    
    partial void OnSelectedActorChanged(ActorItemViewModel? value)
    {
        if (value?.Data.Textures.Count > 0)
        {
            SelectedTextureName = value.Data.Textures.Keys.FirstOrDefault();
            UpdateSelectedTexture();
        }
        else
        {
            SelectedTextureName = null;
            SelectedTexture = null;
        }
    }
    
    partial void OnSelectedTextureNameChanged(string? value)
    {
        UpdateSelectedTexture();
    }
    
    private void UpdateSelectedTexture()
    {
        if (SelectedActor != null && !string.IsNullOrEmpty(SelectedTextureName))
        {
            if (SelectedActor.Data.Textures.TryGetValue(SelectedTextureName, out var texture))
            {
                var vm = new TextureItemViewModel(texture);
                vm.RefreshAtlasRegions();
                SelectedTexture = vm;
            }
            else
            {
                SelectedTexture = null;
            }
        }
        else
        {
            SelectedTexture = null;
        }
    }
    
    [RelayCommand]
    private void AddActor()
    {
        var newActor = new ActorData
        {
            Id = $"actor_{Actors.Count + 1}",
            DisplayName = $"Actor {Actors.Count + 1}"
        };
        
        var vm = new ActorItemViewModel(newActor);
        Actors.Add(vm);
        SelectedActor = vm;
        StatusMessage = "Added new actor";
    }
    
    [RelayCommand]
    private void DeleteActor()
    {
        if (SelectedActor == null) return;
        
        var index = Actors.IndexOf(SelectedActor);
        Actors.Remove(SelectedActor);
        
        SelectedActor = Actors.Count > 0 
            ? (index < Actors.Count ? Actors[index] : Actors[^1])
            : null;
        
        StatusMessage = "Deleted actor";
    }
    
    [RelayCommand]
    private void AddTexture()
    {
        if (SelectedActor == null) return;
        
        var newName = $"texture_{SelectedActor.Data.Textures.Count + 1}";
        var newTexture = new ActorTextures
        {
            AtlasPath = "",
            AtlasRegions = new Dictionary<string, RectLike>
            {
                // Add a default region so there's something to edit
                ["default"] = new RectLike { X = 0, Y = 0, Width = 128, Height = 128 }
            }
        };
        
        SelectedActor.Data.Textures.Add(newName, newTexture);
        SelectedActor.RefreshTextureKeys();
        SelectedTextureName = newName;
        StatusMessage = $"Added texture: {newName}";
    }
    
    [RelayCommand]
    private void DeleteTexture()
    {
        if (SelectedActor == null || string.IsNullOrEmpty(SelectedTextureName)) return;
        
        SelectedActor.Data.Textures.Remove(SelectedTextureName);
        SelectedActor.RefreshTextureKeys();
        SelectedTextureName = SelectedActor.Data.Textures.Keys.FirstOrDefault();
        StatusMessage = "Deleted texture";
    }
    
    [RelayCommand]
    private void AddAtlasRegion()
    {
        if (SelectedTexture == null) return;
        
        var regionCount = SelectedTexture.Data.AtlasRegions.Count;
        var newRegionName = $"region_{regionCount + 1}";
        
        // Make sure the name is unique
        while (SelectedTexture.Data.AtlasRegions.ContainsKey(newRegionName))
        {
            regionCount++;
            newRegionName = $"region_{regionCount + 1}";
        }
        
        var newRegion = new RectLike { X = 0, Y = 0, Width = 128, Height = 128 };
        SelectedTexture.Data.AtlasRegions.Add(newRegionName, newRegion);
        SelectedTexture.RefreshAtlasRegions();
        
        StatusMessage = $"Added atlas region: {newRegionName}";
    }
    
    [RelayCommand]
    private void DeleteAtlasRegion(string? regionName)
    {
        if (SelectedTexture == null || string.IsNullOrEmpty(regionName)) return;
        
        if (SelectedTexture.Data.AtlasRegions.Remove(regionName))
        {
            SelectedTexture.RefreshAtlasRegions();
            StatusMessage = $"Deleted atlas region: {regionName}";
        }
    }
    
    [RelayCommand]
    private async Task BrowseAtlasPath(Window? window)
    {
        if (window == null || SelectedTexture == null) return;
        
        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Atlas Image",
            AllowMultiple = false,
            FileTypeFilter = new[] 
            { 
                new FilePickerFileType("Image Files") 
                { 
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp" } 
                } 
            }
        });
        
        if (files.Count == 0) return;
        
        var selectedPath = files[0].Path.LocalPath;
        
        // Convert to relative path if we have a project root
        if (!string.IsNullOrEmpty(ProjectRootPath) && selectedPath.StartsWith(ProjectRootPath))
        {
            // Make it relative to project root
            var relativePath = Path.GetRelativePath(ProjectRootPath, selectedPath);
            // Convert to forward slashes for Godot
            relativePath = relativePath.Replace('\\', '/');
            SelectedTexture.AtlasPath = "res://" + relativePath;
        }
        else
        {
            // If no project root or file is outside project, just use the filename
            SelectedTexture.AtlasPath = Path.GetFileName(selectedPath);
        }
        
        StatusMessage = $"Selected atlas: {SelectedTexture.AtlasPath}";
    }
    
    [RelayCommand]
    private async Task SetProjectRoot(Window? window)
    {
        if (window == null) return;
        
        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Godot Project Root Folder",
            AllowMultiple = false
        });
        
        if (folders.Count == 0) return;
        
        ProjectRootPath = folders[0].Path.LocalPath;
        StatusMessage = $"Project root set to: {ProjectRootPath}";
    }
    
    [RelayCommand]
    private async Task OpenFile(Window? window)
    {
        if (window == null) return;
        
        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Actor Data",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
        });
        
        if (files.Count == 0) return;
        
        try
        {
            currentFilePath = files[0].Path.LocalPath;
            var json = await File.ReadAllTextAsync(currentFilePath);
            
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            
            var loadedActors = JsonConvert.DeserializeObject<List<ActorData>>(json, settings);
            
            if (loadedActors != null)
            {
                Actors.Clear();
                foreach (var actor in loadedActors)
                {
                    Actors.Add(new ActorItemViewModel(actor));
                }
                SelectedActor = Actors.FirstOrDefault();
                StatusMessage = $"Loaded: {Path.GetFileName(currentFilePath)}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task SaveFile(Window? window)
    {
        if (currentFilePath == null)
        {
            await SaveFileAs(window);
            return;
        }
        
        try
        {
            var actorData = Actors.Select(vm => vm.Data).ToList();
            
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            
            var json = JsonConvert.SerializeObject(actorData, settings);
            await File.WriteAllTextAsync(currentFilePath, json);
            
            StatusMessage = $"Saved: {Path.GetFileName(currentFilePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task SaveFileAs(Window? window)
    {
        if (window == null) return;
        
        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Actor Data",
            DefaultExtension = "json",
            SuggestedFileName = "actors.json",
            FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
        });
        
        if (file == null) return;
        
        currentFilePath = file.Path.LocalPath;
        await SaveFile(window);
    }
}
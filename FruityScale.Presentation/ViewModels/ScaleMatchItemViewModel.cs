using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FruityScale.Domain.Models;
using FruityScale.Domain.MusicTheory;

namespace FruityScale.Presentation.ViewModels;

public partial class ScaleMatchItemViewModel : ObservableObject
{
    public ScaleMatchResult Result { get; }

    [ObservableProperty]
    private bool _isExpanded;
    
    public List<string> ScaleNotes { get; }
    public List<string> WrongNotes { get; }
    public List<string> MissingNotes { get; }
    public List<string> UserNotes { get; }
    
    public List<PianoKeyViewModel> KeyboardVisualization { get; }

    public bool HasWrongNotes => WrongNotes.Any();
    public bool HasMissingNotes => MissingNotes.Any();
    public string ScoreColor => Result.Score switch
    {
        >= 1.0 => "#34D399",   
        >= 0.7 => "#F59E0B",   
        _ => "#EF4444"    
    };

    public string ScoreBackgroundColor => Result.Score switch
    {
        >= 1.0 => "#1A2E26",
        >= 0.7 => "#2D251A",
        _ => "#2D1A1A"
    };

    public ScaleMatchItemViewModel(ScaleMatchResult result)
    {
        Result = result;
        
        ScaleNotes = result.Scale.Intervals
            .Select(interval => NoteNameNormalizer.GetNoteName(result.RootNote + interval))
            .Distinct()
            .ToList();
        
        WrongNotes = result.WrongNotes?
            .Select(noteNum => NoteNameNormalizer.GetNoteName(noteNum))
            .Distinct()
            .ToList() ?? new List<string>();
        
        MissingNotes = result.MissingNotes?
            .Select(noteNum => NoteNameNormalizer.GetNoteName(noteNum))
            .Distinct()
            .ToList() ?? new List<string>();
        
        var correctPlayedNotes = ScaleNotes.Except(MissingNotes);
        UserNotes = correctPlayedNotes
            .Union(WrongNotes)
            .OrderBy(note => Array.IndexOf(MusicConstants.NoteNamesSharp, note))
            .ToList();
        
        KeyboardVisualization = MusicConstants.NoteNamesSharp.Select(note => 
        {
            bool isPlayed = UserNotes.Contains(note);
            bool isWrong = WrongNotes.Contains(note);
            bool isMissing = MissingNotes.Contains(note);
            bool isInScale = ScaleNotes.Contains(note);

            return new PianoKeyViewModel(note, isPlayed, isWrong, isMissing, isInScale);
        }).ToList();
    }

    [RelayCommand]
    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }
}

// Helper class for displaying virtual piano keyboard
public class PianoKeyViewModel
{
    public string NoteName { get; }
    public bool IsWhiteKey { get; }
    
    public string BackgroundColor { get; }
    public string BorderColor { get; }
    public string TextColor { get; }
    public double KeyHeight { get; }

    public PianoKeyViewModel(string noteName, bool isPlayed, bool isWrong, bool isMissing, bool isInScale)
    {
        NoteName = noteName;
        IsWhiteKey = !noteName.Contains('#');
        KeyHeight = IsWhiteKey ? 80 : 50; // makes black keys shorter
        
        if (isWrong)
        {
            BackgroundColor = "#2D1A1A";
            BorderColor = "#EF4444";
            TextColor = "#FFCACC";
        }
        else if (isPlayed && !isWrong)
        {
            BackgroundColor = "#1E293B";
            BorderColor = "#3B82F6";
            TextColor = "#93C5FD";
        }
        else if (isMissing)
        {
            BackgroundColor = "#2D251A";
            BorderColor = "#F59E0B";
            TextColor = "#FDE68A";
        }
        else 
        {
            // note out of scale or isn't played
            BackgroundColor = IsWhiteKey ? "#E5E7EB" : "#121214";
            BorderColor = "#3F3F46";
            TextColor = IsWhiteKey ? "#121214" : "#9CA3AF";
        }
    }
}
using FruityScale.Domain.Enums;

namespace FruityScale.Application.Contracts;

public interface IEnvironmentService
{
    AppPlatform CurrentPlatform { get; }
    string DefaultFlStudioPath { get; }
    
    string AppFolder { get; }
    string LogFilePath { get; }
    string ConfigFilePath { get; }
    string ScaleLibraryPath { get; }
}
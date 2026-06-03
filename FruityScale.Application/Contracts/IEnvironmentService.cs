using FruityScale.Domain.Enums;

namespace FruityScale.Application.Contracts;

public interface IEnvironmentService
{
    AppPlatform CurrentPlatform { get; }
    string DefaultFlStudioPath { get; }
}
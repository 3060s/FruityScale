using FruityScale.Domain.Models;

namespace FruityScale.Application.Contracts;

public interface IScaleProvider
{
    Task<List<ScaleDefinition>> GetScalesAsync();
}
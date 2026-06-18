using FruityScale.Domain.Models;

namespace FruityScale.Application.Contracts;

public interface ISettingsService
{
    UserSettings Current { get; }
    
    void Update(Action<UserSettings> updateAction);
}
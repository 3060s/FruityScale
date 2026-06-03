using FruityScale.Application.Contracts;
using FruityScale.Domain.Enums;

namespace FruityScale.Infrastructure.Services;

public class EnvironmentService : IEnvironmentService
{
    public AppPlatform CurrentPlatform
    {
        get
        {
            if (OperatingSystem.IsWindows()) return AppPlatform.Windows;
            if (OperatingSystem.IsMacOS()) return AppPlatform.MacOS;
            if (OperatingSystem.IsLinux()) return AppPlatform.Linux;
            return AppPlatform.Unknown;
        }
    }

    public string DefaultFlStudioPath
    {
        get
        {
            return CurrentPlatform switch
            {
                // TODO: these paths aren't always the same even on same OS https://github.com/3060s/FruityScale/issues/6
                AppPlatform.Windows => @"C:\Program Files\Image-Line\FL Studio",
                // Not sure about the path because I don't have any MacOS device
                AppPlatform.MacOS => "/Applications/FL Studio.app",
                // FL Studio doesn't work natively on Linux so we leave something like that
                // because it's possible to run through Wine
                AppPlatform.Linux => "~/.wine/drive_c/Program Files/Image-Line/FL Studio",
                _ => "Select your 'Piano roll scripts' directory"
            };
        }
    }
}
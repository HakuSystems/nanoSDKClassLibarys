using JetBrains.Annotations;

namespace nanoEditor.Models;

public class NanoAuthData
{
    [CanBeNull] public string AuthKey { get; set; }
    [CanBeNull] public string Password { get; set; }
}
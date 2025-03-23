#pragma warning disable IDE0130 // Namespace does not match folder structure

// HACK: This enables support for `record` type in .net standard 2.0
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

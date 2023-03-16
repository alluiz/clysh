namespace Clysh.Core;

public static class ClyshConstants
{
    public const string CommandPattern = @"^[a-z]+(\.{0,1}[a-z0-9]+-{0,1})*[a-z0-9]+$";
    public const string OptionPattern = @"^[a-z]+(-{0,1}[a-z0-9]+)+$";
    public const string GroupPattern = @"^[a-z]+(-{0,1}[a-z0-9]+)+$";
    public const string ParameterPattern = @"^[a-zA-Z]+(_{0,1}[a-zA-Z0-9]+)+$";
}
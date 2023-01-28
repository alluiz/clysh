using System.Text.RegularExpressions;

namespace Clysh.Helper;

public static class ClyshMessages
{
    public const string MessageInvalidId = "Invalid ID: The ID must follow the pattern: {0}. ID: '{1}'";
    public const string ErrorOnCreateCommand = "Error on create command. Command: {0}";
    public const string InvalidShortcutReserved = "Shortcut 'h' is reserved to help shortcut. Option: {0}";
    public const string ErrorOnCreateOption = "Error on create option. Option: {0}";
    public const string InvalidParameterRequiredOrder = "Invalid parameter order. The required parameters must come first than optional parameters. Check the order. Parameter: {0}";
    public const string InvalidParameterOrder = "Invalid parameter order. The order must be greater than the lastOrder: {0}. Parameter: {1}";
    public const string ErrorOnCreateParameter = "Error on create parameter. Parameter: {0}";
    public const string ErrorOnValidateMaxLength = "Invalid max length. The max length must be greater than min length.";
    public const string MessageCommandMustHaveOnlyOneParentCommand = "Invalid command: The command must have only one parent. Command: '{0}'";
    public const string MessageOptionAddressMemory = "Invalid command: The option memory address is already related to another command. Option: '{0}'";
    public const string MessageGroupAddressMemory = "Invalid command: The group memory address is already related to another command. Group: '{0}'";
    public const string MessageGroupAddressMemoryIsDifferent = "Invalid command: The group memory address is different between command and option. Group: '{0}'";
    public const string MessageInvalidDescription = "Invalid command: Command description must be not null or empty and between {0} and {1} chars. Description: '{2}'";
    public const string InvalidShorcutMessage = "Invalid shortcut. The shortcut must be null or follow the pattern {0} and between {1} and {2} chars. Shortcut: '{3}'";
    public const string InvalidDescription = "Option description must be not null or empty and between {0} and {1} chars. Description: '{2}'";
    public const string InvalidOrder = "All parameters must be greater or equal than 0 order. OrderValue: {0}";
    public const string InvalidLength = "Invalid min length. The values must be between {0} and {1}.";
    public const string YourCommandDoesNotHaveAnActionConfigured = "Your command does NOT have an action configured. Command: '{0}'.";
    public const string YourCommandDoesNotHaveASubcommandConfigured = "Your command does NOT have a subcommand configured. Command: '{0}'.";
    public const string InvalidOption = "The option '{0}' is invalid.";
    public const string InvalidSubcommand = "You need to provide some subcommand to command '{0}'";
    public const string InvalidArgument = "You can't put parameters without any option that accept it '{0}'";
    public const string InvalidParameter = "The parameter data '{0}' is out of bound for option: {1}.";
    public const string IncorrectParameter = "The parameter '{0}' is invalid for option: {1}.";
    public const string ParameterConflict = "The parameter '{0}' is already filled for option: {1}.";
    public const string RequiredParameters = "Required parameters [{0}] is missing for option: {1} (shortcut: {2})";
    public const string MessageErrorOnBindAction = "Error on bind action with commandId '{0}'. The id was not found.";
    public const string MessageErrorOnLoad = "Error on load data from file path. Path: '{0}'";
    public const string MessageErrorOnCreateCommand = "Error on create command. Command: {0}";
    public const string MessageErrorOnCreateCommands = "Error on create commands from extracted data. Path: '{0}'";
    public const string MessageInvalidCommandsDuplicated = "Invalid commands: The id(s): {0} must be unique. Check your schema and try again.";
    public const string MessageInvalidCommandsDuplicatedRoot = "Invalid commands: Data must have one root command. Consider marking only one command with 'Root': true.";
    public const string MessageInvalidCommandsParent = "Invalid commands: The commands '{0}' does not have a parent. Check if all your commands has a valid parent.";
    public const string MessageInvalidCommandsLengthAtLeastOne = "Invalid commands: The data must contains at least one command.";
    public const string MessageInvalidCommandsNotFound = "Invalid commands: The commandId '{0}' was not found on commands data list.";
    public const string MessageInvalidFileExtension = "Invalid file: Only JSON (.json) and YAML (.yml or .yaml) files are supported. Path: '{0}'";
    public const string MessageInvalidFileJson = "Invalid file: The JSON deserialization results in null object. JSON file path: '{0}'";
    public const string MessageInvalidFilePath = "Invalid file: CLI data file was not found. Path: '{0}'";
    public const string ErrorOnCreateSubCommand = "Error on create subcommand. Subcommand: '{0}'";
    public const string RequiredSubCommand = "The command is configured to require subcommand. So subcommands cannot be null.";
    public const string MessageInvalidIdLength = "Invalid ID: The ID must be less or equal than {0} chars. ID: '{1}'";
    public const string InvalidCommandDuplicatedWords = "Invalid command: The commandId cannot have duplicated words. Command: {command.Id}";
    public const string QuestionMustBeNotBlank = "Question must be not blank";

    public static bool Match(string message, string messagePattern)
    {
        var regex = new Regex("{[0-9]+}");
        var escapeChars = new string[] { ".", "[", "]", "(", ")" };

        messagePattern = escapeChars.Aggregate(messagePattern, (current, escapeChar) => current.Replace(escapeChar, $"\\{escapeChar}"));

        messagePattern = regex.Replace(messagePattern, ".*");
        
        regex = new Regex(messagePattern);

        return regex.IsMatch(message);
    }
    
    public static bool Match(string message, string messagePattern, params string[] values)
    {
        var expectedMessage = string.Format(messagePattern, values);

        return expectedMessage.Equals(message);
    }
}
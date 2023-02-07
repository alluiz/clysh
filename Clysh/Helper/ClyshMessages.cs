using System.Text.RegularExpressions;

namespace Clysh.Helper;

public static class ClyshMessages
{
    public const string ErrorOnBuildCommand = "Error on build command. Command: '{0}'";
    public const string ErrorOnCreateCommand = "Error on create command. Command: '{0}'";
    public const string ErrorOnCreateOption = "Error on create option. Option: '{0}'";
    public const string ErrorOnCreateParameter = "Error on create parameter. Parameter: '{0}'";
    public const string ErrorOnCreateSubCommand = "Error on create subcommand. Subcommand: '{0}'";
    public const string ErrorOnGetOptionFromGroupNotFound = "Error on option from group. The group is not found. Group: '{0}'";
    public const string ErrorOnValidateCommandAction = "Error on validate command. The command does NOT have an action configured. Command: '{0}'.";
    public const string ErrorOnValidateCommandId = "Error on validate command. The ID must not have duplicated words. Command: '{0}'";
    public const string ErrorOnValidateCommandGroupNotFound = "Error on validate command. The group is not found. Group: '{0}'";
    public const string ErrorOnValidateCommandGroupDuplicated = "Error on validate command. The group is duplicated. Group: '{0}'";
    public const string ErrorOnValidateCommandPropertyMemory = "Error on validate command. The memory address is already related to another command. Object: '{0}'";
    public const string ErrorOnValidateCommandParent = "Error on validate command parent. The command must have only one parent. Command: '{0}'";
    public const string ErrorOnValidateCommandSubcommands = "Error on validate command. The ABSTRACT command does NOT have any subcommand configured. Command: '{0}'.";
    public const string ErrorOnValidateDescription = "Error on validate description. The description must be not null or empty and between {0} and {1} chars. Description: '{2}'";
    public const string ErrorOnValidateIdLength = "Error on validate ID. The ID must be less or equal than {0} chars. ID: '{1}'";
    public const string ErrorOnValidateIdPattern = "Error on validate ID. The ID must follow the pattern: '{0}'. ID: '{1}'";
    public const string ErrorOnValidateOptionShortcut = "Error on validate option. The shortcut '{0}' is reserved. Option: '{1}'";
    public const string ErrorOnValidateParameterMaxLength = "Error on validate parameter. The max length must be greater than min length. Parameter: '{0}'";
    public const string ErrorOnValidateParameterRange = "Error on validate parameter. The min and max length must be between {0} and {1}, respectively.";
    public const string ErrorOnValidateParameterOrder = "Error on validate parameter. The order must be greater than the PREVIOUS order and must be greater or equal than 0. Parameter: '{0}'";
    public const string ErrorOnValidateParameterRequiredOrder = "Error on validate parameter. The required parameters must come first than optional parameters. Check the order. Parameter: '{0}'";
    public const string ErrorOnValidateShorcut = "Error on validate shortcut. The shortcut can be null or follow the pattern {0} and between {1} and {2} chars. Shortcut: '{3}'";
    public const string ErrorOnValidateUserInputArgument = "The argument '{0}' is invalid. See --help";
    public const string ErrorOnValidateUserInputArgumentOutOfBound = "The argument '{0}' is out of bound for option '{1}'. See --help";
    public const string ErrorOnValidateUserInputOption = "The option '{0}' is invalid. See --help";
    public const string ErrorOnValidateUserInputParameterConflict = "The parameter '{0}' is already filled for option '{1}'. See --help";
    public const string ErrorOnValidateUserInputParameterInvalid = "The parameter '{0}' is invalid for option '{1}'. See --help";
    public const string ErrorOnValidateUserInputQuestionAnswer = "Question must be not blank";
    public const string ErrorOnValidateUserInputSubcommand = "The command '{0}' requires a subcommand. See --help";
    public const string ErrorOnValidateUserInputRequiredParameters = "The required parameters ['{0}'] is missing for option '{1}' (shortcut: '{2}'). See --help";
    public const string ErrorOnSetupBindAction = "Error on bind action. The command id was not found. Id '{0}'";
    public const string ErrorOnSetupCommands = "Error on setup commands from extracted data. Path: '{0}'";
    public const string ErrorOnSetupCommandsDuplicated = "Error on setup commands. The id(s): '{0}' must be unique. Check your schema and try again.";
    public const string ErrorOnSetupCommandsDuplicatedRoot = "Error on setup commands. Data must have one root command. Consider marking only one command with 'Root': true.";
    public const string ErrorOnSetupCommandsParent = "Error on setup commands. The commands '{0}' does not have a parent. Check if all your commands has a valid parent.";
    public const string ErrorOnSetupCommandsLength = "Error on setup commands. The data must contains at least one command.";
    public const string ErrorOnSetupCommandsNotFound = "Error on setup commands. The commandId '{0}' was not found on commands data list.";
    public const string ErrorOnSetupLoad = "Error on load data. Path: '{0}'";
    public const string ErrorOnSetupLoadFileExtension = "Error on load data. Only JSON (.json) and YAML (.yml or .yaml) files are supported. Path: '{0}'";
    public const string ErrorOnSetupLoadFileJson = "Error on load data. The JSON deserialization results in null object. JSON file path: '{0}'";
    public const string ErrorOnSetupLoadFilePath = "Error on load data. The CLI data file was not found. Path: '{0}'";
    public const string ErrorOnSetupSubCommands = "Error on setup subcommands. The command is configured to require subcommand. Therefore, subcommands must not be null. Command: '{0}'";
    public const string ErrorOnSetupGlobalOptions = "Error on setup global option. The global option must have at least one command. GlobalOption: '{0}'";

    public static bool Match(string message, string messagePattern)
    {
        var regex = new Regex("{[0-9]+}");
        var escapeChars = new[] { ".", "[", "]", "(", ")" };

        messagePattern = escapeChars.Aggregate(messagePattern, (current, escapeChar) => current.Replace(escapeChar, $"\\{escapeChar}"));

        messagePattern = regex.Replace(messagePattern, ".*");
        
        regex = new Regex(messagePattern);

        return regex.IsMatch(message);
    }
    
    public static bool Match(string message, string messagePattern, params object?[] values)
    {
        var expectedMessage = string.Format(messagePattern, values);

        return expectedMessage.Equals(message);
    }
}
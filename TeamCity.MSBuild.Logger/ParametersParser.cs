namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ParametersParser : IParametersParser
    {
        private static readonly char[] ParameterDelimiters = { ';' };
        private static readonly char[] ParameterValueSplitCharacter = { '=' };

        public bool TryParse(string parametersString, Parameters parameters, out string error)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            error = null;
            if (parametersString == null)
            {
                return true;
            }

            foreach (var parameter in parametersString.Split(ParameterDelimiters))
            {
                if (parameter.Length <= 0)
                {
                    continue;
                }

                var paramList = parameter.Split(ParameterValueSplitCharacter);
                if(!ApplyParameter(parameters, paramList[0], paramList.Length > 1 ? paramList[1] : null, out error))
                {
                    return false;
                }
            }

            parameters.AlignMessages = false;
            parameters.BufferWidth = -1;
            if (!parameters.ForceNoAlign)
            {
                try
                {
                    parameters.BufferWidth = Console.BufferWidth;
                    parameters.AlignMessages = true;
                }
                catch (Exception)
                {
                    parameters.AlignMessages = false;
                }
            }

            return true;
        }

        private static bool ApplyParameter([NotNull] Parameters parameters, [NotNull] string parameterName, [CanBeNull] string parameterValue, out string error)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
            var parameterNameUpper = parameterName.ToUpperInvariant();
            error = null;
            switch (parameterNameUpper)
            {
                case "DEBUG":
                    parameters.Debug = true;
                    return true;

                case "WARNINGSONLY":
                    parameters.ShowOnlyWarnings = true;
                    return true;

                case "SHOWENVIRONMENT":
                    parameters.ShowEnvironment = true;
                    return true;

                case "DISABLECONSOLECOLOR":
                    parameters.ColorMode = ColorMode.NoColor;
                    return true;

                case "FORCECONSOLECOLOR":
                    parameters.ColorMode = ColorMode.AnsiColor;
                    return true;

                case "TEAMCITY":
                    parameters.TeamCityMode = TeamCityMode.SupportHierarchy;
                    parameters.StatisticsMode = StatisticsMode.TeamCity;
                    parameters.ColorMode = ColorMode.TeamCity;
                    parameters.ColorThemeMode = ColorThemeMode.TeamCity;
                    parameters.ForceNoAlign = true;
                    parameters.AlignMessages = false;
                    parameters.ShowSummary = true;
                    return true;

                case "V":
                case "VERBOSITY":
                    if (TryApplyVerbosityParameter(parameterValue, out error, out var verbosity))
                    {
                        parameters.Verbosity = verbosity;
                        return true;
                    }
                    return false;

                case "PERFORMANCESUMMARY":
                    parameters.ShowPerfSummary = true;
                    return true;

                case "NOITEMANDPROPERTYLIST":
                    parameters.ShowItemAndPropertyList = false;
                    return true;

                case "NOSUMMARY":
                    parameters.ShowSummary = false;
                    return true;

                case "ERRORSONLY":
                    parameters.ShowOnlyErrors = true;
                    return true;

                case "SHOWPROJECTFILE":
                    parameters.ShowProjectFile = parameterValue == null || (parameterValue.Length == 0 || parameterValue.ToUpperInvariant() == "TRUE");
                    return true;

                case "SUMMARY":
                    parameters.ShowSummary = true;
                    return true;

                case "SHOWCOMMANDLINE":
                    parameters.ShowCommandLine = true;
                    return true;

                case "SHOWTIMESTAMP":
                    parameters.ShowTimeStamp = true;
                    return true;

                case "SHOWEVENTID":
                    parameters.ShowEventId = true;
                    return true;

                case "FORCENOALIGN":
                    parameters.ForceNoAlign = true;
                    parameters.AlignMessages = false;
                    return true;

                default:
                    error = $"Invalid parameter \"{parameterName}\"=\"{parameterValue ?? "null"}\"";
                    return false;
            }
        }

        private static bool TryApplyVerbosityParameter(string parameterValue, out string error, out LoggerVerbosity verbosity)
        {
            var parameterValueUpper = parameterValue.ToUpperInvariant();
            error = null;
            switch (parameterValueUpper)
            {
                case "N":
                case "NORMAL":
                    verbosity = LoggerVerbosity.Normal;
                    return true;

                case "M":
                case "MINIMAL":
                    verbosity = LoggerVerbosity.Minimal;
                    return true;

                case "DIAG":
                case "DIAGNOSTIC":
                    verbosity = LoggerVerbosity.Diagnostic;
                    return true;

                case "D":
                case "DETAILED":
                    verbosity = LoggerVerbosity.Detailed;
                    return true;

                case "Q":
                case "QUIET":
                    verbosity = LoggerVerbosity.Quiet;
                    return true;

                default:
                    error = $"Invalid verbosity \"{parameterValue}\"";
                    verbosity = default(LoggerVerbosity);
                    return false;
            }
        }
    }
}

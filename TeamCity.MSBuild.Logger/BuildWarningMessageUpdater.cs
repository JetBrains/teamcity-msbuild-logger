namespace TeamCity.MSBuild.Logger
{
    using JetBrains.TeamCity.ServiceMessages;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using Microsoft.Build.Framework;

    internal class BuildWarningMessageUpdater: IServiceMessageUpdater
    {
        private readonly IEventContext _eventContext;

        public BuildWarningMessageUpdater(IEventContext eventContext) => 
            _eventContext = eventContext;

        public IServiceMessage UpdateServiceMessage(IServiceMessage message)
        {
            if (_eventContext.TryGetEvent(out var buildEventManager)
                && buildEventManager is BuildWarningEventArgs warning)
            {
                var newMessage = new PatchedServiceMessage(message);
                if (!string.IsNullOrWhiteSpace(warning.Code))
                {
                    newMessage.Add("code", warning.Code);
                }
                
                if (!string.IsNullOrWhiteSpace(warning.File))
                {
                    newMessage.Add("file", warning.File);
                }
                
                if (!string.IsNullOrWhiteSpace(warning.Subcategory))
                {
                    newMessage.Add("subcategory", warning.Subcategory);
                }
                
                if (!string.IsNullOrWhiteSpace(warning.ProjectFile))
                {
                    newMessage.Add("projectFile", warning.ProjectFile);
                }
                
                if (!string.IsNullOrWhiteSpace(warning.SenderName))
                {
                    newMessage.Add("senderName", warning.SenderName);
                }
                
                newMessage.Add("columnNumber", warning.ColumnNumber.ToString());
                newMessage.Add("endColumnNumber", warning.EndColumnNumber.ToString());
                newMessage.Add("lineNumber", warning.LineNumber.ToString());
                newMessage.Add("endLineNumber", warning.EndLineNumber.ToString());
                return newMessage;
            }

            return message;
        }
    }
}
namespace TeamCity.MSBuild.Logger
{
    using JetBrains.TeamCity.ServiceMessages;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using Microsoft.Build.Framework;

    internal class BuildMessageMessageUpdater: IServiceMessageUpdater
    {
        private readonly IEventContext _eventContext;

        public BuildMessageMessageUpdater(IEventContext eventContext) => 
            _eventContext = eventContext;

        public IServiceMessage UpdateServiceMessage(IServiceMessage message)
        {
            if (_eventContext.TryGetEvent(out var buildEventManager)
                && buildEventManager is BuildMessageEventArgs msg)
            {
                var newMessage = new PatchedServiceMessage(message);
                if (!string.IsNullOrWhiteSpace(msg.Code))
                {
                    newMessage.Add("code", msg.Code);
                }
                
                if (!string.IsNullOrWhiteSpace(msg.File))
                {
                    newMessage.Add("file", msg.File);
                }
                
                if (!string.IsNullOrWhiteSpace(msg.Subcategory))
                {
                    newMessage.Add("subcategory", msg.Subcategory);
                }
                
                if (!string.IsNullOrWhiteSpace(msg.ProjectFile))
                {
                    newMessage.Add("projectFile", msg.ProjectFile);
                }
                
                if (!string.IsNullOrWhiteSpace(msg.SenderName))
                {
                    newMessage.Add("senderName", msg.SenderName);
                }
                
                newMessage.Add("columnNumber", msg.ColumnNumber.ToString());
                newMessage.Add("endColumnNumber", msg.EndColumnNumber.ToString());
                newMessage.Add("lineNumber", msg.LineNumber.ToString());
                newMessage.Add("endLineNumber", msg.EndLineNumber.ToString());
                newMessage.Add("importance", msg.Importance.ToString());

                return newMessage;
            }

            return message;
        }
    }
}
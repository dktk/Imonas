// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application.Features.Notifications.DTOs
{
    public class NotificationDto : IMapFrom<Notification>
    {
        public int Id { get; set; }
        public string EmailSmtpServer { get; set; }
        public string EmailSmtpPort { get; set; }
        public string EmailFromAddress { get; set; }
        public bool EmailEnableSsl { get; set; }

        public string SlackWebhookURL { get; set; }
        public string SlackDefaultChannel { get; set; }

        public string MicrosoftTeamsWebhookURL { get; set; }

        public int NotifyOnRunToCompletion { get; set; }
        public int NotifyOnRunFailure { get; set; }
        public int NotifyOnLowMatchRate { get; set; }
        public int NotifyOnCaseSLABreach { get; set; }
        public int NotifyOnCriticalCaseCreated { get; set; }

        public bool ShouldNotifyOnRunToCompletion { get; set; }
        public bool ShouldNotifyOnRunFailure { get; set; }
        public bool ShouldNotifyOnLowMatchRate { get; set; }
        public bool ShouldNotifyOnCaseSLABreach { get; set; }
        public bool ShouldNotifyOnCriticalCaseCreated { get; set; }

    }
}

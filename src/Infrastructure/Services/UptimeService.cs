// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common.Interfaces;
using Application;

namespace Infrastructure.Services
{
    // todo: store the start time in the DB 
    public class UptimeService : IUptimeService
    {
        private readonly DateTime _dateTime;
        public UptimeService(IDateTime dateTime)
        {
            _dateTime = dateTime.Now;
        }

        public TimeSpan GetUptime(IDateTime dateTime) => _dateTime - dateTime.Now;

        public string GetUptimeString(IDateTime dateTime)
        {
            var diff = GetUptime(dateTime);

            var days = diff.Days;
            var hours = diff.Hours;
            var minutes = diff.Minutes;

            if (days > 0)
            {
                return $"{days} days, {hours} hs";
            }

            if (hours > 0)
            {
                return $"{hours} hs, {minutes} mins";
            }

            return $"{minutes} mins";
        }
    }
}

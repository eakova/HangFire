﻿using System;
using System.Collections.Generic;
using ServiceStack.Redis;

namespace HangFire.States
{
    public class ProcessingState : JobState
    {
        public static readonly string Name = "Processing";

        public ProcessingState(string reason, string serverName) 
            : base(reason)
        {
            ServerName = serverName;
        }

        public string ServerName { get; private set; }

        public override string StateName { get { return Name; } }

        public override IDictionary<string, string> GetProperties(JobDescriptor descriptor)
        {
            return new Dictionary<string, string>
                {
                    { "StartedAt", JobHelper.ToStringTimestamp(DateTime.UtcNow) },
                    { "ServerName", ServerName }
                };
        }

        public override void Apply(JobDescriptor descriptor, IRedisTransaction transaction)
        {
            transaction.QueueCommand(x => x.AddItemToSortedSet(
                "hangfire:processing", descriptor.JobId, JobHelper.ToTimestamp(DateTime.UtcNow)));
        }

        public class Descriptor : JobStateDescriptor
        {
            public override void Unapply(JobDescriptor descriptor, IRedisTransaction transaction)
            {
                transaction.QueueCommand(x => x.RemoveItemFromSortedSet(
                    "hangfire:processing", descriptor.JobId));
            }
        }
    }
}
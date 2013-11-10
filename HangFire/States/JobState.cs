﻿using System.Collections.Generic;
using ServiceStack.Redis;

namespace HangFire.States
{
    public abstract class JobState
    {
        protected JobState(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }

        public abstract string StateName { get; }

        public abstract void Apply(JobDescriptor descriptor, IRedisTransaction transaction);
        
        public virtual IDictionary<string, string> GetProperties(JobDescriptor descriptor)
        {
            return new Dictionary<string, string>();
        }
    }
}
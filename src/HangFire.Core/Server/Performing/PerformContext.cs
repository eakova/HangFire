﻿// This file is part of HangFire.
// Copyright © 2013-2014 Sergey Odinokov.
// 
// HangFire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// HangFire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with HangFire. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using HangFire.Common;
using HangFire.Storage;

namespace HangFire.Server.Performing
{
    /// <summary>
    /// Provides information about the context in which the job
    /// is being performed.
    /// </summary>
    public class PerformContext : WorkerContext
    {
        internal PerformContext(PerformContext context)
            : this(context, context.Connection, context.JobId, context.MethodData)
        {
            Items = context.Items;
        }

        internal PerformContext(
            WorkerContext workerContext, 
            IStorageConnection connection, 
            string jobId,
            MethodData methodData)
            : base(workerContext)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (jobId == null) throw new ArgumentNullException("jobId");
            if (methodData == null) throw new ArgumentNullException("methodData");

            Connection = connection;
            JobId = jobId;
            MethodData = methodData;
            Items = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets an instance of the key-value storage. You can use it
        /// to pass additional information between different client filters
        /// or just between different methods.
        /// </summary>
        public IDictionary<string, object> Items { get; private set; }

        public string JobId { get; private set; }
        public MethodData MethodData { get; private set; }

        public IStorageConnection Connection { get; private set; }

        public void SetJobParameter(string name, object value)
        {
            Connection.SetJobParameter(JobId, name, JobHelper.ToJson(value));
        }

        public T GetJobParameter<T>(string name)
        {
            return JobHelper.FromJson<T>(Connection.GetJobParameter(JobId, name));
        }
    }
}

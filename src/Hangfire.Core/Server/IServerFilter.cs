﻿// This file is part of Hangfire. Copyright © 2013-2014 Sergey Odinokov.
// 
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
// REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
// INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
// LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
// OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
// PERFORMANCE OF THIS SOFTWARE.

namespace Hangfire.Server
{
    /// <summary>
    /// Defines methods that are required for a server filter.
    /// </summary>
    public interface IServerFilter
    {
        /// <summary>
        /// Called before the performance of the job.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        void OnPerforming(PerformingContext filterContext);

        /// <summary>
        /// Called after the performance of the job.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        void OnPerformed(PerformedContext filterContext);
    }
}
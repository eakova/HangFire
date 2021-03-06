﻿using System;
using TechTalk.SpecFlow;
using Xunit;

namespace HangFire.Tests
{
    [Binding]
    public class QueueSteps : Steps
    {
        public const string DefaultQueue = "default";
        
        [Given(@"an empty queue")]
        public void GivenAnEmptyQueue()
        {
        }

        [Given(@"an enqueued job")]
        public void GivenAnEnqueuedJob()
        {
            Given(String.Format("a job in the '{0}' queue", DefaultQueue));
        }

        [Given(@"an enqueued broken job")]
        public void GivenAnEnqueuedBrokenJob()
        {
            Given(String.Format("a job of the '{0}' type", typeof(BrokenJob).AssemblyQualifiedName));

            Redis.Client.EnqueueItemOnList(
                String.Format("hangfire:queue:{0}", DefaultQueue),
                JobSteps.DefaultJobId);
        }

        [Given(@"the '(\w+)' job, that was enqueued")]
        public void GivenAnUnexistingEnqueuedJob(string jobId)
        {
            Redis.Client.EnqueueItemOnList(
                String.Format("hangfire:queue:{0}", DefaultQueue),
                jobId);
        }

        [Given(@"a job in the '(.+)' queue")]
        public void GivenAJobInTheQueue(string queue)
        {
            Given("a job");

            Redis.Client.EnqueueItemOnList(
                String.Format("hangfire:queue:{0}", queue),
                JobSteps.DefaultJobId);
        }

        [Given(@"the '(.+)' job in the queue")]
        public void GivenTheJobInTheQueue(string jobId)
        {
            Given(String.Format("the '{0}' job in the '{1}' queue", jobId, DefaultQueue));
        }

        [Given(@"the '(.+)' job in the '(.+)' queue")]
        public void GivenTheJobInTheQueue(string jobId, string queue)
        {
            Given(String.Format("the '{0}' job", jobId));

            Redis.Client.EnqueueItemOnList(
                String.Format("hangfire:queue:{0}", queue),
                jobId);
        }

        [Then(@"the queue should contain the job")]
        public void ThenTheQueueContainsTheJob()
        {
            Then(String.Format("the '{0}' queue should contain the job", DefaultQueue));
        }

        [Then(@"the '(.+)' queue should contain the job")]
        public void ThenTheQueueContainsTheJob(string queue)
        {
            var jobIds = Redis.Client.GetAllItemsFromList(
                String.Format("hangfire:queue:{0}", queue));

            Assert.Contains(JobSteps.DefaultJobId, jobIds);
        }

        [Then(@"the queue should not contain the job anymore")]
        [Then(@"the queue should not contain the job")]
        public void ThenTheQueueDoesNotContainTheJob()
        {
            Then(String.Format("the '{0}' queue should not contain the job", DefaultQueue));
        }

        [Then(@"the '(.+)' queue should not contain the job")]
        public void ThenTheQueueDoesNotContainTheJob(string queue)
        {
            var jobIds = Redis.Client.GetAllItemsFromList(
                String.Format("hangfire:queue:{0}", queue));

            Assert.DoesNotContain(JobSteps.DefaultJobId, jobIds);
        }

        [Then(@"the '(.+)' queue should be empty")]
        public void ThenTheQueueIsEmpty(string queue)
        {
            var length = Redis.Client.GetListCount(
                String.Format("hangfire:queue:{0}", queue));

            Assert.Equal(0, length);
        }

        [Then(@"the '(.+)' queue length should be (\d+)")]
        public void ThenTheQueueLengthIs(string queue, int length)
        {
            var actualLength = Redis.Client.GetListCount(
                String.Format("hangfire:queue:{0}", queue));

            Assert.Equal(length, actualLength);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HangFire.Common;
using HangFire.Redis;
using HangFire.Server;
using HangFire.Server.Performing;
using TechTalk.SpecFlow;
using Xunit;

namespace HangFire.Tests
{
    [Binding]
    public class WorkerManagerSteps : Steps
    {
        private readonly IList<object> _filters = new List<object>();

        private readonly IList<string> _serverResults = new List<string>();
        private readonly IList<string> _exceptionResults = new List<string>();
        private Dictionary<string, string> _parameters;

        [BeforeScenario]
        public void BeforeScenario()
        {
            TestJob.Performed = false;
            TestJob.Disposed = false;
        }

        [Given(@"a manager")]
        public void GivenAManager()
        {
            
        }

        [Given(@"a server filter '(\w+)'")]
        public void GivenAServerFilter(string name)
        {
            _filters.Add(new TestFilter(name, _serverResults));
        }

        [Given(@"a server filter '(\w+)' that cancels the performing")]
        public void GivenAServerFilterThatCancelsThePerforming(string name)
        {
            _filters.Add(new TestFilter(name, _serverResults, false, true));
        }

        [Given(@"a server filter '(\w+)' that throws an exception")]
        public void GivenAServerFilterThatThrowsAnException(string name)
        {
            _filters.Add(new TestFilter(name, _serverResults, true));
        }

        [Given(@"a server filter '(\w+)' that handles an exception")]
        public void GivenAServerFilterThatHandlesAnException(string name)
        {
            _filters.Add(new TestFilter(name, _serverResults, false, false, true));
        }

        [Given(@"the server filter '(\w+)' that sets the following parameters:")]
        public void GivenTheServerFilterThatSetsTheFollowingParameters(string name, Table table)
        {
            _parameters = table.Rows.ToDictionary(x => x["Name"], x => x["Value"]);

            _filters.Add(
                new TestFilter(name, _serverResults, setOnPreMethodParameters: _parameters));
        }

        [Given(@"the server filter '(\w+)' that gets the following parameters:")]
        public void GivenTheClientFilterThatGetsTheFollowingParametersInTheOnCreatingMethod(string name, Table table)
        {
            _parameters = table.Rows.ToDictionary(x => x["Name"], x => x["Value"]);
            GivenTheServerFilterThatReadsAllOfTheAboveParameters(name);
        }

        [Given(@"the server filter '(\w+)' that reads all of the above parameters")]
        public void GivenTheServerFilterThatReadsAllOfTheAboveParameters(string name)
        {
            _filters.Add(new TestFilter(name, _serverResults, readParameters: _parameters));
        }

        [Given(@"a server exception filter '(\w+)'")]
        public void GivenAServerExceptionFilter(string name)
        {
            _filters.Add(new TestExceptionFilter(name, _exceptionResults));
        }

        [Given(@"a server exception filter '(\w+)' that handles an exception")]
        public void GivenAServerExceptionFilterThatHandlesAnException(string name)
        {
            _filters.Add(new TestExceptionFilter(name, _exceptionResults, true));
        }

        /*[When(@"the manager processes the next job")]
        public void WhenTheWorkerPerformsTheJob()
        {
            var context = new ServerContext(
                ServerSteps.DefaultServerName,
                new [] { QueueSteps.DefaultQueue },
                new JobPerformer(_filters));

            using (var manager = new WorkerManager(Redis.Storage, context, 1))
            {
                //manager.ProcessNextJob(new CancellationTokenSource().Token);
            }
        }*/

        [Then(@"the job should be performed")]
        public void ThenTheJobShouldBePerformed()
        {
            Assert.True(TestJob.Performed);
        }

        [Then(@"the job should not be performed")]
        public void ThenTheJobShouldNotBePerformed()
        {
            Assert.False(TestJob.Performed);
        }

        [Then(@"there should be no performing actions")]
        public void ThenThereShouldBeNoPerformingActions()
        {
        }

        [Then(@"the job should be disposed")]
        public void ThenTheJobShouldBeDisposed()
        {
            Assert.True(TestJob.Disposed);
        }

        [Then(@"only the following server filter methods should be executed:")]
        [Then(@"the server filter methods should be executed in the following order:")]
        public void ThenTheServerFilterMethodsShouldBeExecutedInTheFollowingOrder(Table table)
        {
            Assert.Equal(table.RowCount, _serverResults.Count);

            for (var i = 0; i < table.RowCount; i++)
            {
                var method = table.Rows[i]["Method"];
                Assert.Equal(method, _serverResults[i]);
            }
        }

        [Then("the server exception filter should be executed")]
        public void ThenTheClientFilterWasExecuted()
        {
            Assert.NotEqual(0, _exceptionResults.Count);
        }

        [Then("the following server exception filters should be executed:")]
        [Then("the server exception filters should be executed in the following order:")]
        public void ThenTheClientExceptionFiltersWereExecuted(Table table)
        {
            Assert.Equal(table.RowCount, _exceptionResults.Count);

            for (var i = 0; i < table.RowCount; i++)
            {
                var filter = table.Rows[i]["Filter"];
                Assert.Equal(filter, _exceptionResults[i]);
            }
        }

        [Then(@"the last ArticleId should be equal to (\d+)")]
        public void ThenTheLastArticleIdShouldBeEqualTo(int value)
        {
            Assert.Equal(value, CustomJob.LastArticleId);
        }

        [Then(@"the last Author should be equal to '(.+)'")]
        public void ThenTheLastAuthorShouldBeEqualTo(string value)
        {
            Assert.Equal(value, CustomJob.LastAuthor);
        }

        [Then(@"the '(\w+)' server filter got the actual values of the parameters")]
        public void ThenTheServerFilterGotTheActualValuesOfTheParameters(string name)
        {
            // Assert methods are called in the TestFilter class
        }

        [Then(@"the job should have all of the above parameters encoded as JSON string")]
        public void ThenItShouldHaveAllOfTheAboveParametersEncodedAsJsonString()
        {
            var job = Redis.Client.GetAllEntriesFromHash(String.Format("hangfire:job:{0}", JobSteps.DefaultJobId));
            foreach (var parameter in _parameters)
            {
                Assert.True(job.ContainsKey(parameter.Key));
                Assert.Equal(parameter.Value, JobHelper.FromJson<string>(job[parameter.Key]));
            }
        }
    }
}

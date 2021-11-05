namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Surrogate
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Quartz;

    [ExcludeFromCodeCoverage]
    internal class JobDetailSurrogate : IJobDetail
    {
        private readonly JobDataMap _jobDataMap;

        public JobDetailSurrogate(JobDataMap jobDataMap)
        {
            this._jobDataMap = jobDataMap;
        }

        public bool ConcurrentExecutionDisallowed => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public bool Durable => throw new NotImplementedException();
        JobDataMap IJobDetail.JobDataMap => this._jobDataMap;
        public Type JobType => throw new NotImplementedException();
        public JobKey Key => throw new NotImplementedException();
        public bool PersistJobDataAfterExecution => throw new NotImplementedException();
        public bool RequestsRecovery => throw new NotImplementedException();

        public IJobDetail Clone()
        {
            throw new NotImplementedException();
        }

        public JobBuilder GetJobBuilder()
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace KafkaFlow.Retry.Common.Sample.Helpers;

public static class KafkaHelper
{
    public static async Task CreateKafkaTopics(string kafkaBrokers, string[] topics)
    {
        using (var adminClient =
               new AdminClientBuilder(new AdminClientConfig { BootstrapServers = kafkaBrokers }).Build())
        {
            foreach (var topic in topics)
            {
                var topicMetadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(20));
                if (topicMetadata.Topics.First().Partitions.Count > 0)
                {
                    try
                    {
                        var deleteTopicRecords = new List<Confluent.Kafka.TopicPartitionOffset>();
                        for (var i = 0; i < topicMetadata.Topics.First().Partitions.Count; i++)
                        {
                            deleteTopicRecords.Add(new Confluent.Kafka.TopicPartitionOffset(topic, i, Offset.End));
                        }

                        await adminClient.DeleteRecordsAsync(deleteTopicRecords).ConfigureAwait(false);
                    }
                    catch (DeleteRecordsException e)
                    {
                        Console.WriteLine($"An error occured deleting topic records: {e.Results[0].Error.Reason}");
                    }
                }
                else
                {
                    try
                    {
                        await adminClient
                            .CreatePartitionsAsync(
                                new List<PartitionsSpecification>
                                {
                                    new()
                                    {
                                        Topic = topic,
                                        IncreaseTo = 6
                                    }
                                })
                            .ConfigureAwait(false);
                    }
                    catch (CreateTopicsException e)
                    {
                        if (e.Results[0].Error.Code != ErrorCode.UnknownTopicOrPart)
                        {
                            Console.WriteLine($"An error occured creating a topic: {e.Results[0].Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine("Topic does not exists");
                        }
                    }
                }
            }
        }
    }
}
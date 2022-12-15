---
sidebar_position: 1
description: KafkaFlow Retry Extensions for Apache Kafka consumer\'s resilience.
sidebar_label: Introduction
slug: /
---

# Introduction to KafkaFlow Retry Extensions

🔁 KafkaFlow Retry is an extension to [KafkaFlow](https://github.com/Farfetch/kafkaflow) that implements resilience on Apache Kafka consumers.

Get started by installing [KafkaFlow Retry Extensions](getting-started/installation) or following our [Quickstart](getting-started/quickstart).

## Features {#features}

Our goal is to simplify building resilient Apache Kafka consumers when using KafkaFlow. 

To do that, KafkaFlow Retry Extensions gives you access to retry policies like:
 - Simple Retry
 - Forever Retry
 - Durable Retry

**Durable Retries** let you keep processing while keeping the faulty message offset persisted on **SqlServer** or **MongoDb**.You can also expose an HTTP API to manage those messages.

## Join the community {#join-the-community}

- [GitHub](https://github.com/farfetch/kafkaflow-retry-extensions): For new feature requests, bug reporting or contributing with your own pull request.

## Something missing here? {#something-missing}

If you have suggestions to improve the documentation, please send us a new [issue](https://github.com/farfetch/kafkaflow-retry-extensions/issues).


## License

KafkaFlow Retry Extensions is a free and open-source project, released under the permissible [MIT license](https://github.com/Farfetch/kafkaflow/blob/master/LICENSE). 
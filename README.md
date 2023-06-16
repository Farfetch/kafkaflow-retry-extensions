# KafkaFlow Retry Extensions · [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/farfetch/kafkaflow-retry-extensions/blob/main/LICENSE) [![nuget version](https://img.shields.io/nuget/v/kafkaflow.retry.svg?style=flat)](https://www.nuget.org/packages/KafkaFlow.Retry/) ![Build Main](https://github.com/Farfetch/kafkaflow-retry-extensions/workflows/Build/badge.svg?branch=main) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/2a86b45f0ec2487fb63dfd581071465a)](https://www.codacy.com/gh/Farfetch/kafkaflow-retry-extensions/dashboard?utm_source=github.com&utm_medium=referral&utm_content=Farfetch/kafkaflow-retry-extensions&utm_campaign=Badge_Grade)

## Introduction

🔁 KafkaFlow Retry is an extension to [KafkaFlow](https://github.com/Farfetch/kafkaflow) that implements resilience on Apache Kafka consumers.

Want to give it a try? Check out our [Quickstart](https://farfetch.github.io/kafkaflow-retry-extensions/getting-started/quickstart)!

### Resilience policies

| Policy                                                                               | Description                                                                                                                                                                                                                                                                                                                      |                                                       Aka                                                      | Required Packages |
| ------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------: | ----------------- |
| **Simple Retry** <br/>(policy family)<br/><sub>([quickstart](#simple) ; deep)</sub>  | Many faults are transient and may self-correct after a short delay.                                                                                                                                                                                                                                                              |                                            "Maybe it's just a blip"                                            | KafkaFlow.Retry   |
| **Forever Retry**<br/>(policy family)<br/><sub>([quickstart](#forever) ; deep)</sub> | Many faults are semi-transient and may self-correct after multiple retries.                                                                                                                                                                                                                                                      |                                                 "Never give up"                                                | KafkaFlow.Retry   |
| **Durable Retry**<br/><sub>([quickstart](#durable) ; deep)</sub>                     | Beyond a certain amount of retries and waiting, you want to keep processing next-in-line messages but you can't lose the current offset message. As persistence databases, MongoDb or SqlServer is available. And you can manage in-retry messages through HTTP API."I can't stop processing messages but I can't lose messages" | KafkaFlow.Retry <br/>KafkaFlow.Retry.API<br/><br/>KafkaFlow.Retry.SqlServer<br/>or<br/>KafkaFlow.Retry.MongoDb |                   |

## Installation

[Read the docs](https://farfetch.github.io/kafkaflow-retry-extensions/getting-started/installation) for any further information.

## Documentation

Learn more about using KafkaFlow Retry Extensions [here](https://farfetch.github.io/kafkaflow-retry-extensions/)!

## Contributing

Read our [contributing guidelines](CONTRIBUTING.md) to learn about our development process, how to propose bugfixes and improvements, and how to build and test your changes.

## Maintainers

-   [Bruno Gomes](https://github.com/brunohfgomes)
-   [Carlos Miranda](https://github.com/carlosgoias)
-   [Fernando Marins](https://github.com/fernando-a-marins)
-   [Leandro Magalhães](https://github.com/spookylsm)
-   [Luís Garcês](https://github.com/luispfgarces)
-   [Martinho Novais](https://github.com/martinhonovais)
-   [Rodrigo Belo](https://github.com/rodrigobelo)
-   [Sérgio Ribeiro](https://github.com/sergioamribeiro)

## Get in touch

You can find us at:

-   [GitHub Issues](https://github.com/Farfetch/kafkaflow-retry-extensions/issues)

## License

KafkaFlow Retry is a free and open source project, released under the permissible [MIT license](LICENSE).

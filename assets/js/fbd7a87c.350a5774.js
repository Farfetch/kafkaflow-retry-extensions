"use strict";(self.webpackChunkwebsite=self.webpackChunkwebsite||[]).push([[39],{3905:(e,t,a)=>{a.d(t,{Zo:()=>d,kt:()=>m});var n=a(7294);function r(e,t,a){return t in e?Object.defineProperty(e,t,{value:a,enumerable:!0,configurable:!0,writable:!0}):e[t]=a,e}function o(e,t){var a=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),a.push.apply(a,n)}return a}function l(e){for(var t=1;t<arguments.length;t++){var a=null!=arguments[t]?arguments[t]:{};t%2?o(Object(a),!0).forEach((function(t){r(e,t,a[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(a)):o(Object(a)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(a,t))}))}return e}function s(e,t){if(null==e)return{};var a,n,r=function(e,t){if(null==e)return{};var a,n,r={},o=Object.keys(e);for(n=0;n<o.length;n++)a=o[n],t.indexOf(a)>=0||(r[a]=e[a]);return r}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)a=o[n],t.indexOf(a)>=0||Object.prototype.propertyIsEnumerable.call(e,a)&&(r[a]=e[a])}return r}var i=n.createContext({}),c=function(e){var t=n.useContext(i),a=t;return e&&(a="function"==typeof e?e(t):l(l({},t),e)),a},d=function(e){var t=c(e.components);return n.createElement(i.Provider,{value:t},e.children)},p="mdxType",u={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},k=n.forwardRef((function(e,t){var a=e.components,r=e.mdxType,o=e.originalType,i=e.parentName,d=s(e,["components","mdxType","originalType","parentName"]),p=c(a),k=r,m=p["".concat(i,".").concat(k)]||p[k]||u[k]||o;return a?n.createElement(m,l(l({ref:t},d),{},{components:a})):n.createElement(m,l({ref:t},d))}));function m(e,t){var a=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var o=a.length,l=new Array(o);l[0]=k;var s={};for(var i in t)hasOwnProperty.call(t,i)&&(s[i]=t[i]);s.originalType=e,s[p]="string"==typeof e?e:r,l[1]=s;for(var c=2;c<o;c++)l[c]=a[c];return n.createElement.apply(null,l)}return n.createElement.apply(null,a)}k.displayName="MDXCreateElement"},2708:(e,t,a)=>{a.r(t),a.d(t,{assets:()=>i,contentTitle:()=>l,default:()=>p,frontMatter:()=>o,metadata:()=>s,toc:()=>c});var n=a(7462),r=(a(7294),a(3905));const o={sidebar_position:2,sidebar_label:"Quickstart"},l="Quickstart: Apply your first Retry Policy",s={unversionedId:"getting-started/quickstart",id:"getting-started/quickstart",title:"Quickstart: Apply your first Retry Policy",description:"In this article, you use C# and the .NET CLI to create two applications that will produce and consume events from Apache Kafka.",source:"@site/docs/getting-started/quickstart.md",sourceDirName:"getting-started",slug:"/getting-started/quickstart",permalink:"/kafkaflow-retry-extensions/getting-started/quickstart",draft:!1,editUrl:"https://github.com/farfetch/kafkaflow-retry-extensions/tree/main/website/docs/getting-started/quickstart.md",tags:[],version:"current",sidebarPosition:2,frontMatter:{sidebar_position:2,sidebar_label:"Quickstart"},sidebar:"tutorialSidebar",previous:{title:"Installation",permalink:"/kafkaflow-retry-extensions/getting-started/installation"},next:{title:"Packages",permalink:"/kafkaflow-retry-extensions/getting-started/packages"}},i={},c=[{value:"Prerequisites",id:"prerequisites",level:2},{value:"Overview",id:"overview",level:2},{value:"Steps",id:"steps",level:2},{value:"1. Create a folder for your applications",id:"1-create-a-folder-for-your-applications",level:3},{value:"2. Setup Apache Kafka",id:"2-setup-apache-kafka",level:3},{value:"3. Start the cluster",id:"3-start-the-cluster",level:3},{value:"4. Create Producer Project",id:"4-create-producer-project",level:3},{value:"5. Install KafkaFlow packages",id:"5-install-kafkaflow-packages",level:3},{value:"6. Create the Message contract",id:"6-create-the-message-contract",level:3},{value:"7. Create message sender",id:"7-create-message-sender",level:3},{value:"8. Create Consumer Project",id:"8-create-consumer-project",level:3},{value:"9. Add a reference to the Producer",id:"9-add-a-reference-to-the-producer",level:3},{value:"10. Install KafkaFlow packages",id:"10-install-kafkaflow-packages",level:3},{value:"11. Install KafkaFlow Retry Extensions packages",id:"11-install-kafkaflow-retry-extensions-packages",level:3},{value:"12. Create a Message Handler",id:"12-create-a-message-handler",level:3},{value:"13. Create the Message Consumer",id:"13-create-the-message-consumer",level:3},{value:"14. Run!",id:"14-run",level:3}],d={toc:c};function p(e){let{components:t,...a}=e;return(0,r.kt)("wrapper",(0,n.Z)({},d,a,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"quickstart-apply-your-first-retry-policy"},"Quickstart: Apply your first Retry Policy"),(0,r.kt)("p",null,"In this article, you use C# and the .NET CLI to create two applications that will produce and consume events from Apache Kafka.\nThe consumer will use a Simple Retry strategy to retry in case of a given exception type."),(0,r.kt)("p",null,"By the end of the article, you will know how to use KafkaFlow Retry Extensions to make your Consumers resilient."),(0,r.kt)("h2",{id:"prerequisites"},"Prerequisites"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"https://dotnet.microsoft.com/en-us/download/dotnet/6.0"},".NET 6.0 SDK")),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"https://www.docker.com/products/docker-desktop/"},"Docker Desktop"))),(0,r.kt)("h2",{id:"overview"},"Overview"),(0,r.kt)("p",null,"You will create two applications:"),(0,r.kt)("ol",null,(0,r.kt)("li",{parentName:"ol"},(0,r.kt)("strong",{parentName:"li"},"Consumer:")," Will be running waiting for incoming messages and will write them to the console. The Message Handler will randomly fail, but the execution will retry."),(0,r.kt)("li",{parentName:"ol"},(0,r.kt)("strong",{parentName:"li"},"Producer:")," Will send a message every time you run the application.")),(0,r.kt)("p",null,"To connect them, you will be running an Apache Kafka cluster using Docker."),(0,r.kt)("h2",{id:"steps"},"Steps"),(0,r.kt)("h3",{id:"1-create-a-folder-for-your-applications"},"1. Create a folder for your applications"),(0,r.kt)("p",null,"Create a new folder with the name ",(0,r.kt)("em",{parentName:"p"},"KafkaFlowRetryQuickstart"),"."),(0,r.kt)("h3",{id:"2-setup-apache-kafka"},"2. Setup Apache Kafka"),(0,r.kt)("p",null,"Inside the folder from step 1, create a ",(0,r.kt)("inlineCode",{parentName:"p"},"docker-compose.yml")," file. You can download it from ",(0,r.kt)("a",{parentName:"p",href:"https://github.com/Farfetch/kafkaflow/blob/master/docker-compose.yml"},"here"),"."),(0,r.kt)("h3",{id:"3-start-the-cluster"},"3. Start the cluster"),(0,r.kt)("p",null,"Using your terminal of choice, start the cluster."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"docker-compose up -d\n")),(0,r.kt)("h3",{id:"4-create-producer-project"},"4. Create Producer Project"),(0,r.kt)("p",null,"Run the following command to create a Console Project named ",(0,r.kt)("em",{parentName:"p"},"Producer"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet new console --name Producer\n")),(0,r.kt)("h3",{id:"5-install-kafkaflow-packages"},"5. Install KafkaFlow packages"),(0,r.kt)("p",null,"Inside the ",(0,r.kt)("em",{parentName:"p"},"Producer")," project directory, run the following commands to install the required packages."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet add package KafkaFlow\ndotnet add package KafkaFlow.Microsoft.DependencyInjection\ndotnet add package KafkaFlow.LogHandler.Console\ndotnet add package KafkaFlow.Serializer.JsonCore\ndotnet add package Microsoft.Extensions.DependencyInjection\n")),(0,r.kt)("h3",{id:"6-create-the-message-contract"},"6. Create the Message contract"),(0,r.kt)("p",null,"Add a new class file named ",(0,r.kt)("em",{parentName:"p"},"HelloMessage.cs")," and add the following example:"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-csharp"},"namespace Producer;\n\npublic class HelloMessage\n{\n    public string Text { get; set; } = default!;\n}\n")),(0,r.kt)("h3",{id:"7-create-message-sender"},"7. Create message sender"),(0,r.kt)("p",null,"Replace the content of the ",(0,r.kt)("em",{parentName:"p"},"Program.cs")," with the following example:"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-csharp"},'using Microsoft.Extensions.DependencyInjection;\nusing KafkaFlow.Producers;\nusing KafkaFlow;\nusing KafkaFlow.Serializer;\nusing Producer;\n\nvar services = new ServiceCollection();\n\nconst string topicName = "sample-topic";\nconst string producerName = "say-hello";\n\nservices.AddKafka(\n    kafka => kafka\n        .UseConsoleLog()\n        .AddCluster(\n            cluster => cluster\n                .WithBrokers(new[] { "localhost:9092" })\n                .CreateTopicIfNotExists(topicName, 1, 1)\n                .AddProducer(\n                    producerName,\n                    producer => producer\n                        .DefaultTopic(topicName)\n                        .AddMiddlewares(m =>\n                            m.AddSerializer<JsonCoreSerializer>()\n                            )\n                )\n        )\n);\n\nvar serviceProvider = services.BuildServiceProvider();\n\nvar producer = serviceProvider\n    .GetRequiredService<IProducerAccessor>()\n    .GetProducer(producerName);\n\nawait producer.ProduceAsync(\n                   topicName,\n                   Guid.NewGuid().ToString(),\n                   new HelloMessage { Text = "Hello!" });\n\n\nConsole.WriteLine("Message sent!");\n\n')),(0,r.kt)("h3",{id:"8-create-consumer-project"},"8. Create Consumer Project"),(0,r.kt)("p",null,"Run the following command to create a Console Project named ",(0,r.kt)("em",{parentName:"p"},"Consumer"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet new console --name Consumer\n")),(0,r.kt)("h3",{id:"9-add-a-reference-to-the-producer"},"9. Add a reference to the Producer"),(0,r.kt)("p",null,"In order to access the message contract, add a reference to the Producer Project."),(0,r.kt)("p",null,"Inside the ",(0,r.kt)("em",{parentName:"p"},"Consumer")," project directory, run the following commands to add the reference."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet add reference ../Producer\n")),(0,r.kt)("h3",{id:"10-install-kafkaflow-packages"},"10. Install KafkaFlow packages"),(0,r.kt)("p",null,"Inside the ",(0,r.kt)("em",{parentName:"p"},"Consumer")," project directory, run the following commands to install the required packages."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet add package KafkaFlow\ndotnet add package KafkaFlow.Microsoft.DependencyInjection\ndotnet add package KafkaFlow.LogHandler.Console\ndotnet add package KafkaFlow.Serializer.JsonCore\ndotnet add package Microsoft.Extensions.DependencyInjection\n")),(0,r.kt)("h3",{id:"11-install-kafkaflow-retry-extensions-packages"},"11. Install KafkaFlow Retry Extensions packages"),(0,r.kt)("p",null,"Inside the ",(0,r.kt)("em",{parentName:"p"},"Consumer")," project directory, run the following commands to install the required packages."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet add package KafkaFlow.Retry\n")),(0,r.kt)("h3",{id:"12-create-a-message-handler"},"12. Create a Message Handler"),(0,r.kt)("p",null,"Create a new class file named ",(0,r.kt)("em",{parentName:"p"},"HelloMessageHandler.cs")," and add the following example."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-csharp"},'using KafkaFlow;\nusing Producer;\n\nnamespace Consumer;\n\npublic class HelloMessageHandler : IMessageHandler<HelloMessage>\n{\n    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());\n    private static bool ShouldFail() => Random.Next(2) == 1;\n\n    public Task Handle(IMessageContext context, HelloMessage message)\n    {\n        if (ShouldFail())\n        {\n            Console.WriteLine(\n                "Let\'s fail: Partition: {0} | Offset: {1} | Message: {2}",\n                context.ConsumerContext.Partition,\n                context.ConsumerContext.Offset,\n                message.Text);\n            throw new IOException();\n        }\n\n        Console.WriteLine(\n            "Partition: {0} | Offset: {1} | Message: {2}",\n            context.ConsumerContext.Partition,\n            context.ConsumerContext.Offset,\n            message.Text);\n\n        return Task.CompletedTask;\n    }\n}\n')),(0,r.kt)("p",null,"As you can see randomly the handler will throw an ",(0,r.kt)("em",{parentName:"p"},"IOException"),"."),(0,r.kt)("h3",{id:"13-create-the-message-consumer"},"13. Create the Message Consumer"),(0,r.kt)("p",null,"Replace the content of the ",(0,r.kt)("em",{parentName:"p"},"Program.cs")," with the following example."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-csharp"},'using KafkaFlow;\nusing Microsoft.Extensions.DependencyInjection;\nusing KafkaFlow.Retry;\nusing KafkaFlow.Serializer;\nusing Consumer;\n\nconst string topicName = "sample-topic";\nvar services = new ServiceCollection();\n\nservices.AddKafka(kafka => kafka\n    .UseConsoleLog()\n    .AddCluster(cluster => cluster\n        .WithBrokers(new[] { "localhost:9092" })\n        .CreateTopicIfNotExists(topicName, 1, 1)\n        .AddConsumer(consumer => consumer\n            .Topic(topicName)\n            .WithAutoOffsetReset(AutoOffsetReset.Earliest)\n            .WithGroupId("sample-group")\n            .WithBufferSize(100)\n            .WithWorkersCount(10)\n            .AddMiddlewares(\n                middlewares => middlewares\n                    .RetrySimple(\n                        (config) => config\n                            .Handle<IOException>()\n                            .TryTimes(3)\n                            .WithTimeBetweenTriesPlan((retryCount) =>\n                                TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 1000)\n                            )\n                    )\n                    .AddDeserializer<JsonCoreDeserializer>()\n                    .AddTypedHandlers(h => h.AddHandler<HelloMessageHandler>())\n            )\n        )\n    )\n);\n\nvar serviceProvider = services.BuildServiceProvider();\n\nvar bus = serviceProvider.CreateKafkaBus();\n\nawait bus.StartAsync();\n\nConsole.ReadKey();\n\nawait bus.StopAsync();\n')),(0,r.kt)("h3",{id:"14-run"},"14. Run!"),(0,r.kt)("p",null,"From the ",(0,r.kt)("inlineCode",{parentName:"p"},"KafkaFlowRetryQuickstart")," directory:"),(0,r.kt)("ol",null,(0,r.kt)("li",{parentName:"ol"},"Run the Consumer:")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet run --project Consumer/Consumer.csproj \n")),(0,r.kt)("ol",{start:2},(0,r.kt)("li",{parentName:"ol"},"From another terminal, run the Producer:")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-bash"},"dotnet run --project Producer/Producer.csproj \n")))}p.isMDXComponent=!0}}]);
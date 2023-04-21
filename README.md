# Question and Answer flow with Semantic Kernel

A sample implementation of a question and answer flow using Semantic Kernel.

This uses sqlite to store embeddings (caution: sqlite is not vector optimized) and OpenAi to answer questions based on the text found in the database.

## How to run

- Get an OpenAI API Key
- Copy & Paste the API key into the appsettings.json or user secrets (`OpenAI:ApiKey`)
- `dotnet run`

## Examples

```
Enter a question or press enter to quit: What is sk?
🤖: Semantic Kernel (SK) is a lightweight SDK that lets you easily mix conventional programming languages with the latest in Large Language Model (LLM) AI "prompts" with templating, chaining, and planning capabilities out-of-the-box.
```

```
Enter a question or press enter to quit: What systems cann SK connect to?
🤖: SK can connect to external APIs, MS Graph Connector Kit, Bing search query, OpenXML streams, and SQLite.
```

```
Enter a question or press enter to quit: Shoe me an example of creating a kernel
🤖: using Microsoft.SemanticKernel;

var myKernel = Kernel.Builder.Build();
```

```
Enter a question or press enter to quit: Was bedeutet SK?
🤖: SK steht für Semantic Kernel.
```

```
Enter a question or press enter to quit: ¿Qué significa SK?
🤖: SK significa Kernel Semántico.
```

## How does it work?

- At the first run the program will download a couple of pages of the Semantic Kernel documentation.
- For each page embeddings will be calculated using the OpenAI Embeddings model.
- The content of the page and the embeddings are stored in a sqlite database.
- When asked a question, embeddings will be calculated for the question.
- The sqlite database will be searched be similar embeddings
- The top 2 results will be concatenated and send to OpenAI with the following prompts

```
Use the following pieces of context to answer the question at the end. If you don't know the answer, don't try to make up an answer and answer with 'I don't know'. Answer in the langauge that used for the question.

{{$context}}

Question: {{$input}}
Answer:
```
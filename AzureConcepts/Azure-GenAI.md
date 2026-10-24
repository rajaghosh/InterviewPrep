Azure OpenAI Access

1. How to take access of Azure OpenAI

---

---

Azure AI Services

Some of the services present (popular one)

 1. Azure OpenAI Account
 2. AI search
 3. Computer Vision
 4. Face API
 5. Speech Service
 6. Language Service
 7. Translator
 8. Document Intelligence
 9. Bot services
10. Anomaly detector

We will discuss on the popular ones.

1. Azure OpenAI Studio
2. Azure AI Studio

Diff between Azure OpenAi vs OpenAi

Azure OpenAi -

With Azure Open Ai customer get the security capabilities of Microsoft Azure while running the same models as OpenAi. Azure OpenAi offers private networking, regional availability and responsible AI content filtering.

Features -

All managed services like alert (for suspecious activity)

OpenAi -

OpenAi oin itself isnt as secure as its Azure ofering. Properties such as private networking , regional availability AI content filtering arent supported/available.

Azure OpenAI is very secure as well

MFA

RBAC

Data Encryption at rest

Azure KV

Service Endpoint

Private Endpoint

Please not whenevr an "azure openAI service" is created all the resource like LLM and region needs to be created

Then click on Model Deploymens → ManageDeployments

\
Azure OpenAi

Not accessible on free account

To create a new Azure OpenAi resource →

1. Basic :Subscription → ResourceGroup → Region → Name → PricingTier
2. Network:
3. Tags :
4. Submit

Check Azure OpenAi documentation - <https://docs.azure.cn/en-us/ai-services/>

Note - Not all models is available in all the regions

1. Also in the documentation it contains cost
2. Fine tune might not be availble in all the region - Ex - South Central US

Also available at NorthCentral US

3. For finetuning models the costs are high
4. RAG is also costly under the package as DB cost is also included.

When a new Azure AI resource is created then -

1. Keys + Endpoints
2. Model Deployments. are under Azure OpenAi studio.(Not Azure AI Studio)

Azure OpenAi studio\
Under the Azure OPENAI studio → Models

Models will be showed

To start the work we need to perform Deployments of models

Under playground we have "chat completion api" (this is another resource called "completion api" both are diff)

To deploy a model. → Check the quota then go ahead

On deployments → Create new deployments → Select the model → Model version → Deployment types (if it is provisioned type then additional subcriptuion is needed)→ Deploument name → Here we habe the concept of tokens per minute rtate limit → create

For chat playground under chat we can see the deployment models

1. Let us go to "Playground Settings" → we have Language (like english) + subscription + Speech resource
2. Then we have the "view code" → It has the endpoint + api key + python code

3\. Under the setup we have-

- Prompt - the Template selection + System Message

- Add your data -

4. We also have under configuration to select the past message included count
   1. Config has temp + top p etc
   2. freq penalty
   3. Presenece penalty

THere is also a DALL-E playground

---

---

\
To work on the python code -

Generate a python project -

then project sturucture is like -

> env
>
> chat_completions.py
>
> [dalle.py → Add the endpoint + apikey + add the python code (Which is present in the portal)](http://dalle.py)
>
> requirements.txt → Add the dependencies like
>
> ```
>            openai
> ```

Note - We can have a PIP install requirements.txt

Please Note the code will be typically like -

dalle.py

import os

from openai import AzureOpenAI

import json

client = AzureOpenAI(

```
 api_version = "2024-02-01",

 azure_endpoint = endpoint,

 api_key = apikey
```

)

result = client.images.generate(

```
 model = "Dalle3",

 prompt = "xxx",

 n =1            #number of image
```

)

image_url = json.loads(result.model_dump_json())\['data'\]\[0\]\['url'\]

print(image_url)

---

---

chat_completion.py

---

---

---

At them Azure portal. We also have option to add a data source

→ Data source can be

Azure AI search

Blob Storage

Cosmos DB, Mongo DB

Elastic Search

Web Add

Upload files

Select Upload Files

Select → Data Source → create a blob storage

Here we can do is a similarity search (keyword similarity and not vector similarity)

Need also to add the chunk size

Start with the document based search

Then we can deplop

New use case → For using voice related task

Deploy a new model. We get the corresponding python code.

Check deployment model types

Understadnig of TPM vs RPM

---

—

Fine Tuning

UserPrompt → Chat Interface → API Call to OpenApi→ User Prompt + Func Description→ Function Calling → ThirdParty API

---

For each of the models it has parameters which is parameters = **weitghts x bias**

→ → → → → → → → → →

Each model is using the transformer as a base model (inside transformers we have neural network which)

, neural network which works with weight and bias. (Weight and bias are trainable)

Now consider a situation my LLM has knowledge till a particular date

And not have data post that date

So we need to perform fine tuning using the parameters. This is additional training of the LLM

To Fine Tune the Model

Open Azure OpenAi Studio → Models → Create a custom model -

Check the region which supports (Like supported in NorthCentralUs and not supported in EastUs)

Please not whenevr an "azure openAI service" is created all the resource like LLM and region needs to be created

Then click on Model Deploymens → ManageDeployments

Under custom model 

Under base model → Select model + Model Suffix

Provide training data → Data can be from blob or can be uploaded. But data has to be in JSON model. 

And data should be of min 10lines

Providattion data 

Fine tuning training per complete hour →

Azure AI studio

Create a new instance →

Create a new Azure AI Hub → Add subscritption + Resource group + Region + Name + Connect to existing Azure AI service base model

When this will be created additional connected resource will be crweated → Keyvault to store keys + Storage account to store data + Azure ML workspace

In the Azure AI Studio we start with creating a project

We have the options → Components

1. Data - Data for work
2. Indexes - Vector DB indexes
3. Deployments - Model Deployments (if already deplyed we can find the details here)
4. 
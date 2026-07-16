## Summary

* We want all code changes in the shippable nuget packages to be human authored.
* LLM usage elsewhere is acceptable, you are expected to thoroughly scrutinise any generated content.
* AI usage should be declared when submitting pull requests.

## Rationale

While we acknowledge that generative AI _can_ produce valuable output and improve velocities, there are a number of drawbacks and risk factors associated with it's usage.

* AI can, and will, produce low quality code contributions and hallucinations if not given sufficient review and steering.
* Reliance on AI will reduce the human understanding of the code.
* Generative output shifts the burden of labour from authoring code to review. It is very easy to become fatigued with excessive review and miss things that ought to be caught during development.
* AI lacks accountability and it is impossible to prove who owns the code that an AI generates.

Given that Open.IdentityServer is a security product, and that specs in the IAM space move at a very slow pace, we feel that these risks far outweigh any benefit.  We do not want AI generated code contributions to the shippable nuget packages.

We are open to AI usage in other areas of the project, eg. drafting documentation, samples, test assemblies, etc.  The human is responsible for all changes, generated output should be thoroughly scrutinized by the author.

AI usage should be declared when submitting pull requests.
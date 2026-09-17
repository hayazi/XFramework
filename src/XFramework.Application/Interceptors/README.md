# Application service interception

The previous interceptor implementation depended on abstractions that were not part of the consolidated application pipeline.

Interception will be reintroduced as a coherent pipeline in a later step. Keeping incomplete interceptor classes in the build would make the framework non-compilable, so the obsolete implementations were removed during consolidation.

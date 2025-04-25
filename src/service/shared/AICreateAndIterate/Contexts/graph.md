# YamlFixPicksIterator Step/Event Flow

This diagram visualizes the step-by-step Picks and event-driven transitions in the YamlFixPicksIterator. Each block represents a Picksing step, and arrows show how events trigger transitions between steps. Event names are labeled on the connections, making it clear how the Picks flows and where human-in-the-loop actions are required.

The `ConsoleKernelPicksMessageChannel` is the communication bridge for human-in-the-loop events. When the Picks emits events such as `WaitingOnHumanIterate`, `WaitingOnHumanReview`, `WaitingOnHumanSaveFile`, or `WaitingOnHumanFinished`, they are sent to this channel. The channel then waits for user input and sends the corresponding event back into the Picks to continue execution.

```mermaid
flowchart TD
    %% App group
    subgraph "App"
        Start([Start])
        Pick([Pick Solution Per Error])
        Review([Approve Fix])
        Systems([Systems])
        NoErrors([No Errors])
    end

    Start --> Pick
    Pick --> Review
    Review -->|Reject Change| Start
    Systems --> NoErrors

 
 

   subgraph "Fix"
     LoadAndValidateStep([LoadAndValidateStep])
        RecommendFirstFixStep([RecommendFirstFixStep])
        SuggestFixForErrorsStep([SuggestFixForErrorsStep])
        FixSyntaxWithLLMStep([FixSyntaxWithLLMStep])
        HumanIterateStep([HumanIterateStep])
        ApplyFixStep([ApplyFixStep])
        ValidateFixStep([ValidateFixStep])
        HumanReviewStep([HumanReviewStep])
        SaveFixStep([SaveFixStep])
        
    AIToIterateStep([AIToIterateStep])
    AIToReviewStep([AIToReviewStep])
    end

    Start -->|Start| LoadAndValidateStep

    LoadAndValidateStep -->|No Errors| NoErrors
    LoadAndValidateStep -->|Syntax Errors| FixSyntaxWithLLMStep
    LoadAndValidateStep -->|Errors| RecommendFirstFixStep

    FixSyntaxWithLLMStep -->|Needs Review| HumanReviewStep

    RecommendFirstFixStep -->|FunctionResult| SuggestFixForErrorsStep

    SuggestFixForErrorsStep -->|FunctionResult| HumanIterateStep
    HumanIterateStep -->|RequestHumanInTheLoopForIterate| Pick

    %% AIToIterateStep is entered via input event from EventChannelStep (AIToIterate)
    AIToIterateStep -->|FunctionResult| ApplyFixStep

    ApplyFixStep -->|FunctionResult| ValidateFixStep


    ValidateFixStep -->|TryToApplyFixAgain| ApplyFixStep
    ValidateFixStep -->|RequestReview| HumanReviewStep
    HumanReviewStep -->|RequestHumanInTheLoopForReview| Review

    %% AIToReviewStep is entered via input event from EventChannelStep (AIToReview)
    AIToReviewStep -->|FunctionResult| SaveFixStep

    ValidateFixStep -->|FunctionResult| HumanReviewStep

    SaveFixStep -->|RequestSystemToSaveFile| Systems

    %% External entries
    Pick -->|Apply Fix to Selected| ApplyFixStep
    

 
   Review -->|Approved Changes| SaveFixStep
 
    %% AI event routing
    Pick -- "AI To Pick" --> AIToIterateStep
    Review -- "AI to Review" --> AIToReviewStep

   
   
```

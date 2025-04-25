namespace AICreateAndIterate.FixErrors.Events
{
    /// <summary>
    /// Centralized event names for process orchestration.
    /// </summary>
    public static class ProcessEvents
    {
        // Entry and process control events
        public const string Start = "Start";
        public const string StartProcess = "LoadAndValidateStep";
        public const string NoErrorsFound = "NoErrorsFound";
        
        public const string FixAnError = "FixAnError";
        public const string FixSyntaxWithLLM = "FixSyntaxWithLLM";
        public const string ApplyFix = "ApplyFix";
        public const string AutoFixReady = "AutoFixReady";
        public const string IterateRequired = "IterateRequired";
        public const string TryToApplyFixAgain = "TryToApplyFixAgain";
        public const string RequestReview = "RequestReview";

        public const string SaveFix = "SaveFix";
        public const string RejectFix = "RejectFix";

        // Human-in-the-loop event triggers
        public const string WaitingOnHumanIterate = "WaitingOnHumanIterate";
        public const string WaitingOnHumanValidateMaxAttempts = "WaitingOnHumanValidateMaxAttempts";
        public const string WaitingOnHumanValidateError = "WaitingOnHumanValidateError";
        public const string WaitingOnHumanReview = "WaitingOnHumanReview";
        public const string WaitingOnHumanFinished = "WaitingOnHumanFinished";
        public const string RequestSystemSaveFile = "RequestSystemSaveFile";


        //AI steps
        public const string AIToIterate = "AIProcessStep";
        public const string AIToReview = "AIProcessStepCompleted";

        // Human request events (for explicit human actions)
        public const string RequestHumanInTheLoopForIterate = "RequestHumanInTheLoopIterate";
        public const string RequestHumanInTheLoopForReview = "RequestHumanInTheLoopValidate";
        public const string RequestHumanInTheLoopForFailure = "RequestHumanInTheLoopFailure";
        public const string RequestHumanToSaveFile = "RequestHumanInSaveFile";

        // All event names (for routing, validation, etc.)
        public static readonly string[] AllEvents =
        [
            Start,
            StartProcess,
            FixSyntaxWithLLM,
            ApplyFix,
            AutoFixReady,
            IterateRequired,
            WaitingOnHumanIterate,
            WaitingOnHumanValidateMaxAttempts,
            WaitingOnHumanValidateError,
 
            WaitingOnHumanReview,
            RequestHumanInTheLoopForIterate,
            RequestHumanInTheLoopForReview,
            RequestHumanInTheLoopForFailure,
            TryToApplyFixAgain,
            RequestReview
        ];

        // Events that require human intervention
        public static readonly string[] HumanInTheLoopEvents =
        [
            WaitingOnHumanIterate,
            WaitingOnHumanValidateMaxAttempts,
            WaitingOnHumanValidateError,
 
            WaitingOnHumanReview,
            RequestSystemSaveFile,
            WaitingOnHumanFinished,
        ];

        // Explicit human request events
        public static readonly string[] HumanRequestEvents =
        [
            RequestHumanInTheLoopForIterate,
            RequestHumanInTheLoopForReview,
            RequestHumanInTheLoopForFailure
        ];
    }
}

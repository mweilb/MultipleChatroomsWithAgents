 

namespace AICreateAndIterate.FixErrors
{
    public class ErrorToFixSolution
    {
        public List<string> Options { get; init; } = [];
        public string EventName { get; init; } = string.Empty;
        public int SelectedIndex { get; set; } = -1;

        public string FixedYaml { get; set; } = string.Empty;
        public int Attempts { get; set; } = 0;
        public string ExplainDifferences { get; set; } = string.Empty;

        public ErrorToFixSolution(
            List<string> options,
            string eventName)
        {
            Options = options;
            EventName = eventName;
        }
    }
}

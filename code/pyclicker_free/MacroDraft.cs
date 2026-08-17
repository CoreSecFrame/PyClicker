using System.Collections.Generic;

namespace PyClickerRecorder
{
    internal sealed class MacroDraft
    {
        public const string RuntimeName = "__CURRENT__";

        public SavedMacro Macro { get; } = new SavedMacro
        {
            Name = RuntimeName,
            ScheduleMode = MacroScheduleMode.None,
            ScheduledTime = new DateTime(2000, 1, 1),
            ScheduledEndTime = new DateTime(2000, 1, 1)
        };

        public void ReplaceActions(IEnumerable<RecordedAction> actions)
        {
            Macro.Actions = new List<RecordedAction>(actions);
        }

        public void Load(SavedMacro source)
        {
            ReplaceActions(source.Actions);
            CopySettings(source, Macro);
        }

        public void CopyTo(SavedMacro destination)
        {
            destination.Actions = new List<RecordedAction>(Macro.Actions);
            CopySettings(Macro, destination);
        }

        private static void CopySettings(SavedMacro source, SavedMacro destination)
        {
            destination.ScheduleMode = source.ScheduleMode;
            destination.ScheduledTime = source.ScheduledTime;
            destination.ScheduledEndTime = source.ScheduledEndTime;
            destination.LoopCount = source.LoopCount;
            destination.InfiniteLoop = source.InfiniteLoop;
            destination.SpeedFactor = source.SpeedFactor;
            destination.RepeatDaily = source.RepeatDaily;
        }
    }
}

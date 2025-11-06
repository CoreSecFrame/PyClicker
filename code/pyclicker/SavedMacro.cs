using System;
using System.Collections.Generic;

namespace PyClickerRecorder
{
    public enum MacroScheduleMode
    {
        SpecificTime,
        TimeRange,
        OnWindowsStartup
    }

    public class SavedMacro
    {
        public string Name { get; set; }
        public List<RecordedAction> Actions { get; set; }
        public MacroScheduleMode ScheduleMode { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public int LoopCount { get; set; }
        public bool InfiniteLoop { get; set; }
        public double SpeedFactor { get; set; }
        public bool IsEnabled { get; set; }
        public bool RepeatDaily { get; set; }

        public SavedMacro()
        {
            Actions = new List<RecordedAction>();
            Name = "New Macro";
            SpeedFactor = 1.0;
            LoopCount = 1;
            ScheduledTime = DateTime.Now;
            ScheduledEndTime = DateTime.Now;
        }
    }
}

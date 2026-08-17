using PyClickerRecorder;

var draft = new MacroDraft();
Assert(draft.Macro.ScheduleMode == MacroScheduleMode.None, "A new draft must not be scheduled.");
draft.Macro.LoopCount = 7;
draft.Macro.InfiniteLoop = true;
draft.Macro.SpeedFactor = 2.5;

var actions = new List<RecordedAction> { new DelayAction { Duration = 0.25 } };
draft.ReplaceActions(actions);

Assert(draft.Macro.LoopCount == 7, "Replacing a recording must preserve its repetition count.");
Assert(draft.Macro.InfiniteLoop, "Replacing a recording must preserve infinite-loop mode.");
Assert(draft.Macro.SpeedFactor == 2.5, "Replacing a recording must preserve its playback speed.");
Assert(draft.Macro.Actions.Count == 1, "The draft must contain the newly recorded actions.");
Assert(!ReferenceEquals(draft.Macro.Actions, actions), "The draft must own its action list.");

var saved = new SavedMacro { Name = "Saved", IsEnabled = true };
draft.CopyTo(saved);

Assert(saved.Name == "Saved", "Saving a draft must preserve the chosen macro name.");
Assert(saved.IsEnabled, "Saving a draft must preserve the saved macro's enabled state.");
Assert(saved.LoopCount == 7 && saved.InfiniteLoop, "Saving must copy repetition settings.");
Assert(saved.SpeedFactor == 2.5, "Saving must copy playback speed.");
Assert(!ReferenceEquals(saved.Actions, draft.Macro.Actions), "A saved macro must own its action list.");

var existing = new SavedMacro
{
    Name = "Existing",
    Actions = new List<RecordedAction> { new DelayAction { Duration = 1 } },
    LoopCount = 3,
    SpeedFactor = 0.5,
    RepeatDaily = true
};
draft.Load(existing);

Assert(draft.Macro.Name == MacroDraft.RuntimeName, "A loaded draft must retain its runtime identity.");
Assert(draft.Macro.LoopCount == 3 && draft.Macro.SpeedFactor == 0.5, "Loading must copy editable settings.");
Assert(draft.Macro.RepeatDaily, "Loading must copy scheduling settings.");
Assert(!ReferenceEquals(draft.Macro.Actions, existing.Actions), "Loading must isolate draft actions from the saved macro.");

Console.WriteLine("MacroDraft tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

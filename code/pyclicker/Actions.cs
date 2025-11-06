using System.Drawing;
using System.Text.Json.Serialization;

namespace PyClickerRecorder
{
    // Base class for all recordable actions.
    // The JsonDerivedType attributes are necessary for the JSON serializer to know
    // which specific class to create when deserializing from a JSON file.
    [JsonDerivedType(typeof(MouseMoveAction), typeDiscriminator: "move")]
    [JsonDerivedType(typeof(MouseClickAction), typeDiscriminator: "click")]
    [JsonDerivedType(typeof(MouseScrollAction), typeDiscriminator: "scroll")]
    [JsonDerivedType(typeof(KeyAction), typeDiscriminator: "key")]
    [JsonDerivedType(typeof(DelayAction), typeDiscriminator: "delay")]
    public abstract class RecordedAction
    {
        // Common property for all actions, can be used for logging or timing.
        public double TimeOffset { get; set; }
    }

    public class MouseMoveAction : RecordedAction
    {
        public Point Position { get; set; }
    }

    public class MouseClickAction : RecordedAction
    {
        public Point Position { get; set; }
        public string Button { get; set; } = string.Empty; // "Left", "Right", "Middle"
        public string State { get; set; } = string.Empty; // "Down", "Up"
    }

    public class MouseScrollAction : RecordedAction
    {
        public Point Position { get; set; }
        public int Amount { get; set; } // Negative for down, positive for up
    }

    public class KeyAction : RecordedAction
    {
        public int Key { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public string State { get; set; } = string.Empty; // "Press", "Release"
    }

    public class DelayAction : RecordedAction
    {
        public double Duration { get; set; } // in seconds
    }
}

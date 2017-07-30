using System;

namespace SoftIdeas.IOHandler.Adaptee
{
    /// <summary>
    /// Flags based enumerator to formalize caller feedback.
    /// </summary>
    [Flags]
    public enum Feedback
    {
        Default = 0,
        Failed = 1,
        Retry = 2,
        Succeed = 4,
        Wait = 8,
        Continue = 16
    };
}

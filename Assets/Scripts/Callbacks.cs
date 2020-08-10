﻿/// <summary>
/// Class for callbacks
/// </summary>
namespace Ghost
{
    public static class Callbacks
    {
        public delegate void CallbackDelegateNull();
        public delegate void CallbackDelegateInt(int intIn);
        public delegate void CallbackDelegateString(string stringIn);
    }
}
